using Microsoft.EntityFrameworkCore;
using NmuShuttle.Api.Data;
using NmuShuttle.Api.Services;

namespace NmuShuttle.Api.BackgroundServices;

/// <summary>
/// Background service that locks trip instances exactly 60 minutes before departure (SAST).
/// After locking, promotes waitlisted bookings to confirmed via IWaitlistPromotionService.
/// Once locked, the BookingService rejects new bookings and cancellations (HTTP 400).
/// </summary>
public class LockoutGateService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<LockoutGateService> _logger;

    /// <summary>
    /// Polling interval when no upcoming trips are found.
    /// </summary>
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(30);

    public LockoutGateService(
        IServiceScopeFactory scopeFactory,
        ILogger<LockoutGateService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("LockoutGateService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessNextLockoutAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LockoutGateService encountered an error. Retrying after poll interval.");
                await Task.Delay(PollInterval, stoppingToken);
            }
        }

        _logger.LogInformation("LockoutGateService stopped.");
    }

    /// <summary>
    /// Finds the next Scheduling trip and sleeps until its lockout time (T-60 min).
    /// </summary>
    private async Task ProcessNextLockoutAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ShuttleDbContext>();

        // Query next trip with Status == "Scheduling" that has the earliest departure
        var nextTrip = await db.TripInstances
            .Include(ti => ti.ScheduleTime)
            .Where(ti => ti.Status == "Scheduling")
            .OrderBy(ti => ti.TripDate)
            .ThenBy(ti => ti.ScheduleTime.DepartureTime)
            .FirstOrDefaultAsync(stoppingToken);

        if (nextTrip == null)
        {
            _logger.LogDebug("No Scheduling trips found. Polling again in {Interval}.", PollInterval);
            await Task.Delay(PollInterval, stoppingToken);
            return;
        }

        // Calculate lockout time: departure (SAST) - 60 minutes, converted to UTC
        var lockoutTimeUtc = CalculateLockoutTimeUtc(nextTrip.TripDate, nextTrip.ScheduleTime.DepartureTime);
        var utcNow = DateTime.UtcNow;

        if (lockoutTimeUtc > utcNow)
        {
            var delay = lockoutTimeUtc - utcNow;
            _logger.LogInformation(
                "Next lockout for TripInstanceId={TripId} at {LockoutTime:u}. Sleeping for {Delay}.",
                nextTrip.TripInstanceId, lockoutTimeUtc, delay);

            await Task.Delay(delay, stoppingToken);
        }

        if (stoppingToken.IsCancellationRequested)
            return;

        // Execute lockout within a transaction
        await LockTripAsync(nextTrip.TripInstanceId, stoppingToken);
    }

    /// <summary>
    /// Locks a trip instance within a SERIALIZABLE transaction and runs waitlist promotion.
    /// </summary>
    private async Task LockTripAsync(int tripInstanceId, CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ShuttleDbContext>();
        var waitlistService = scope.ServiceProvider.GetRequiredService<IWaitlistPromotionService>();

        // Wrap in execution strategy to support retrying + user-initiated transactions
        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable, stoppingToken);

            try
            {
                // Re-fetch the trip with tracking to verify it's still in Scheduling state
                var trip = await db.TripInstances
                    .FirstOrDefaultAsync(ti => ti.TripInstanceId == tripInstanceId, stoppingToken);

                if (trip == null)
                {
                    _logger.LogWarning(
                        "TripInstanceId={TripId} no longer exists. Skipping lockout.", tripInstanceId);
                    await transaction.RollbackAsync(stoppingToken);
                    return;
                }

                if (trip.Status != "Scheduling")
                {
                    _logger.LogInformation(
                        "TripInstanceId={TripId} is no longer Scheduling (Status={Status}). Skipping lockout.",
                        tripInstanceId, trip.Status);
                    await transaction.RollbackAsync(stoppingToken);
                    return;
                }

                // Lock the trip
                trip.Status = "Locked";
                trip.LockedAt = DateTime.UtcNow;

                await db.SaveChangesAsync(stoppingToken);
                await transaction.CommitAsync(stoppingToken);

                _logger.LogInformation(
                    "TripInstanceId={TripId} locked at {LockedAt:u}.", tripInstanceId, trip.LockedAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to lock TripInstanceId={TripId}. Rolling back.", tripInstanceId);
                await transaction.RollbackAsync(stoppingToken);
                throw;
            }
        });

        // Run waitlist promotion after successful lock (outside the serializable transaction)
        try
        {
            await waitlistService.RunAsync(tripInstanceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Waitlist promotion failed for TripInstanceId={TripId} after lockout.", tripInstanceId);
            // Don't re-throw — the trip is already locked, promotion failure shouldn't crash the service
        }
    }

    /// <summary>
    /// Calculates the lockout time in UTC.
    /// Lockout = Departure time (SAST) minus 60 minutes, converted to UTC.
    /// SAST = UTC+2, so lockoutUtc = departureDateTime - 60min - 2hours.
    /// </summary>
    private static DateTime CalculateLockoutTimeUtc(DateOnly tripDate, TimeOnly departureTime)
    {
        // Combine date and time to get full SAST departure DateTime
        var departureSast = tripDate.ToDateTime(departureTime);

        // Lockout is 60 minutes before departure in SAST
        var lockoutSast = departureSast.AddMinutes(-60);

        // Convert SAST to UTC (SAST = UTC + 2)
        var lockoutUtc = lockoutSast.AddHours(-2);

        return lockoutUtc;
    }
}
