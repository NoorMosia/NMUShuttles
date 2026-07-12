using Microsoft.EntityFrameworkCore;
using NmuShuttle.Api.Data;
using NmuShuttle.Api.Models;

namespace NmuShuttle.Api.BackgroundServices;

/// <summary>
/// Nightly background service that fires at midnight SAST (22:00 UTC).
/// Generates TripInstances 3 days ahead and injects priority bookings.
/// </summary>
public class RollingInjectionService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RollingInjectionService> _logger;

    public RollingInjectionService(
        IServiceScopeFactory scopeFactory,
        ILogger<RollingInjectionService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RollingInjectionService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var delayUntilMidnight = CalculateDelayUntilMidnightSast();
            _logger.LogInformation(
                "RollingInjectionService sleeping for {Delay} until next SAST midnight.",
                delayUntilMidnight);

            await Task.Delay(delayUntilMidnight, stoppingToken);

            if (stoppingToken.IsCancellationRequested)
                break;

            try
            {
                await RunInjectionAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "RollingInjectionService encountered an error during execution.");
            }
        }

        _logger.LogInformation("RollingInjectionService stopped.");
    }

    /// <summary>
    /// Calculates the TimeSpan until the next midnight SAST (UTC+2).
    /// </summary>
    private static TimeSpan CalculateDelayUntilMidnightSast()
    {
        var utcNow = DateTime.UtcNow;
        var sastNow = utcNow.AddHours(2); // SAST = UTC+2

        // Next SAST midnight
        var nextMidnightSast = sastNow.Date.AddDays(1); // midnight tomorrow SAST
        var nextMidnightUtc = nextMidnightSast.AddHours(-2); // convert back to UTC

        var delay = nextMidnightUtc - utcNow;

        // If somehow delay is zero or negative (race at exact midnight), wait 24h
        if (delay <= TimeSpan.Zero)
            delay = TimeSpan.FromHours(24);

        return delay;
    }

    /// <summary>
    /// Core injection logic: create TripInstances and priority bookings for TargetDate.
    /// </summary>
    internal async Task RunInjectionAsync(CancellationToken cancellationToken)
    {
        // SAST today + 3 days
        var sastToday = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(2));
        var targetDate = sastToday.AddDays(3);

        _logger.LogInformation("RollingInjectionService executing for TargetDate={TargetDate}.", targetDate);

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ShuttleDbContext>();

        // ── Step 1: Check CalendarExceptions ──
        var isException = await db.CalendarExceptions
            .AnyAsync(ce => ce.TargetDate == targetDate, cancellationToken);

        if (isException)
        {
            _logger.LogInformation(
                "TargetDate {TargetDate} is a calendar exception. Skipping trip generation.", targetDate);
            return;
        }

        // ── Step 2: Skip Sundays ──
        if (targetDate.DayOfWeek == DayOfWeek.Sunday)
        {
            _logger.LogInformation(
                "TargetDate {TargetDate} is a Sunday. Skipping trip generation.", targetDate);
            return;
        }

        // ── Step 3: Determine ScheduleType ──
        var scheduleType = targetDate.DayOfWeek == DayOfWeek.Saturday ? "Saturday" : "Weekday";
        _logger.LogInformation(
            "TargetDate {TargetDate} resolved to ScheduleType={ScheduleType}.", targetDate, scheduleType);

        // ── Step 4: Query matching ScheduleTimes ──
        var scheduleTimes = await db.ScheduleTimes
            .Where(st => st.ScheduleType == scheduleType)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Found {Count} ScheduleTimes for type '{Type}'.", scheduleTimes.Count, scheduleType);

        // ── Step 5: Create TripInstances (idempotent) ──
        var createdTripIds = new List<int>();

        foreach (var scheduleTime in scheduleTimes)
        {
            var tripInstance = new TripInstance
            {
                ScheduleTimeId = scheduleTime.ScheduleTimeId,
                StopId = scheduleTime.StopId,
                TripDate = targetDate,
                Status = "Scheduling"
            };

            db.TripInstances.Add(tripInstance);

            try
            {
                await db.SaveChangesAsync(cancellationToken);
                createdTripIds.Add(tripInstance.TripInstanceId);
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                // Idempotency: trip already exists for this (ScheduleTimeId, StopId, TripDate)
                _logger.LogDebug(
                    "TripInstance already exists for ScheduleTimeId={ScheduleTimeId}, StopId={StopId}, TripDate={TripDate}. Skipping.",
                    scheduleTime.ScheduleTimeId, scheduleTime.StopId, targetDate);
                db.Entry(tripInstance).State = EntityState.Detached;
            }
        }

        _logger.LogInformation("Created {Count} new TripInstances for {TargetDate}.", createdTripIds.Count, targetDate);

        // ── Step 6: Inject priority bookings ──
        var dayOfWeekStr = targetDate.DayOfWeek.ToString(); // e.g., "Monday"

        var prioritySchedules = await db.PrioritySchedules
            .Where(ps => ps.DayOfWeek == dayOfWeekStr && ps.IsActive)
            .ToListAsync(cancellationToken);

        _logger.LogInformation(
            "Found {Count} active PrioritySchedules for {DayOfWeek}.", prioritySchedules.Count, dayOfWeekStr);

        var bookingsCreated = 0;

        foreach (var ps in prioritySchedules)
        {
            // Find matching TripInstance
            var tripInstance = await db.TripInstances
                .FirstOrDefaultAsync(ti =>
                    ti.ScheduleTimeId == ps.ScheduleTimeId &&
                    ti.StopId == ps.StopId &&
                    ti.TripDate == targetDate,
                    cancellationToken);

            if (tripInstance == null)
            {
                _logger.LogWarning(
                    "No TripInstance found for PrioritySchedule {PsId} (ScheduleTimeId={StId}, StopId={SpId}, TripDate={Td}).",
                    ps.PriorityScheduleId, ps.ScheduleTimeId, ps.StopId, targetDate);
                continue;
            }

            // Check if booking already exists for this student on this trip
            var existingBooking = await db.Bookings
                .AnyAsync(b =>
                    b.StudentId == ps.StudentId &&
                    b.TripInstanceId == tripInstance.TripInstanceId,
                    cancellationToken);

            if (existingBooking)
            {
                _logger.LogDebug(
                    "Booking already exists for StudentId={StudentId} on TripInstanceId={TripId}. Skipping.",
                    ps.StudentId, tripInstance.TripInstanceId);
                continue;
            }

            // Find shuttle at OrderPosition=1 (front accessible shuttle)
            int? shuttleId = await db.TripShuttleAllocations
                .Where(a => a.TripInstanceId == tripInstance.TripInstanceId &&
                            a.OrderPosition == 1 &&
                            a.IsOperational)
                .Select(a => (int?)a.ShuttleId)
                .FirstOrDefaultAsync(cancellationToken);

            var booking = new Booking
            {
                StudentId = ps.StudentId,
                TripInstanceId = tripInstance.TripInstanceId,
                BookingStatus = "Confirmed",
                BookingType = "Priority",
                BoardedStatus = "Pending",
                QrCodeToken = Guid.NewGuid().ToString("N"),
                ShuttleId = shuttleId,
                CreatedAt = DateTime.UtcNow
            };

            db.Bookings.Add(booking);
            await db.SaveChangesAsync(cancellationToken);
            bookingsCreated++;
        }

        _logger.LogInformation(
            "RollingInjectionService completed for {TargetDate}. Created {BookingCount} priority bookings.",
            targetDate, bookingsCreated);
    }

    /// <summary>
    /// Detects if a DbUpdateException is caused by a unique constraint violation (SQL Server error 2601/2627).
    /// </summary>
    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        var message = ex.InnerException?.Message ?? ex.Message;
        // SQL Server unique constraint/index violation error numbers
        return message.Contains("2601") || message.Contains("2627") ||
               message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("duplicate", StringComparison.OrdinalIgnoreCase);
    }
}
