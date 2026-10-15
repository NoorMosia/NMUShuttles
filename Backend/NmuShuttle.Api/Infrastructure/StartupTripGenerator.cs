using Microsoft.EntityFrameworkCore;
using NmuShuttle.Api.Data;
using NmuShuttle.Api.Models;

namespace NmuShuttle.Api.Infrastructure;

/// <summary>
/// Generates trips for today through today+3 on first startup.
/// This ensures there's always data available for testing without waiting for the nightly job.
/// Idempotent — skips dates that already have trips generated.
/// </summary>
public static class StartupTripGenerator
{
    public static async Task GenerateTripsAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ShuttleDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ShuttleDbContext>>();

        var sastToday = SastClock.Today;

        logger.LogInformation("StartupTripGenerator: Generating trips for {Today} through {End}.",
            sastToday, sastToday.AddDays(3));

        for (int offset = 0; offset <= 3; offset++)
        {
            var targetDate = sastToday.AddDays(offset);
            await GenerateForDateAsync(db, logger, targetDate);
        }

        logger.LogInformation("StartupTripGenerator: Complete.");
    }

    private static async Task GenerateForDateAsync(ShuttleDbContext db, ILogger logger, DateOnly targetDate)
    {
        // Skip Sundays
        if (targetDate.DayOfWeek == DayOfWeek.Sunday)
        {
            logger.LogDebug("Skipping {Date} — Sunday.", targetDate);
            return;
        }

        // Skip calendar exceptions
        var isException = await db.CalendarExceptions.AnyAsync(ce => ce.TargetDate == targetDate);
        if (isException)
        {
            logger.LogDebug("Skipping {Date} — calendar exception.", targetDate);
            return;
        }

        // Determine schedule type
        var scheduleType = targetDate.DayOfWeek == DayOfWeek.Saturday ? "Saturday" : "Weekday";

        // Get matching schedule times
        var scheduleTimes = await db.ScheduleTimes
            .Where(st => st.ScheduleType == scheduleType)
            .ToListAsync();

        var tripsCreated = 0;

        foreach (var st in scheduleTimes)
        {
            // Check if trip already exists (idempotent)
            var exists = await db.TripInstances.AnyAsync(t =>
                t.ScheduleTimeId == st.ScheduleTimeId &&
                t.StopId == st.StopId &&
                t.TripDate == targetDate);

            if (exists) continue;

            var trip = new TripInstance
            {
                ScheduleTimeId = st.ScheduleTimeId,
                StopId = st.StopId,
                TripDate = targetDate,
                Status = "Scheduling"
            };

            db.TripInstances.Add(trip);

            try
            {
                await db.SaveChangesAsync();
                tripsCreated++;

                // Allocate a shuttle to this trip so it has capacity
                await AllocateShuttleAsync(db, trip);

                // Inject priority bookings for this trip
                await InjectPriorityBookingsAsync(db, trip, targetDate);
            }
            catch (DbUpdateException)
            {
                // Unique constraint violation — already exists, skip
                db.Entry(trip).State = EntityState.Detached;
            }
        }

        // Also allocate shuttles to any existing trips that have no allocations
        await BackfillShuttleAllocationsAsync(db, targetDate);

        if (tripsCreated > 0)
            logger.LogInformation("Created {Count} trips for {Date} ({Type}).", tripsCreated, targetDate, scheduleType);
    }

    private static async Task InjectPriorityBookingsAsync(ShuttleDbContext db, TripInstance trip, DateOnly targetDate)
    {
        var dayOfWeekStr = targetDate.DayOfWeek.ToString();

        var prioritySchedules = await db.PrioritySchedules
            .Where(ps => ps.IsActive &&
                         ps.DayOfWeek == dayOfWeekStr &&
                         ps.ScheduleTimeId == trip.ScheduleTimeId &&
                         ps.StopId == trip.StopId)
            .ToListAsync();

        foreach (var ps in prioritySchedules)
        {
            // Check if booking already exists
            var bookingExists = await db.Bookings.AnyAsync(b =>
                b.StudentId == ps.StudentId &&
                b.TripInstanceId == trip.TripInstanceId);

            if (bookingExists) continue;

            // Find front shuttle (OrderPosition=1)
            int? shuttleId = await db.TripShuttleAllocations
                .Where(a => a.TripInstanceId == trip.TripInstanceId &&
                            a.OrderPosition == 1 &&
                            a.IsOperational)
                .Select(a => (int?)a.ShuttleId)
                .FirstOrDefaultAsync();

            var booking = new Booking
            {
                StudentId = ps.StudentId,
                TripInstanceId = trip.TripInstanceId,
                BookingStatus = "Confirmed",
                BookingType = "Priority",
                BoardedStatus = "Pending",
                QrCodeToken = Guid.NewGuid().ToString("N"),
                ShuttleId = shuttleId,
                CreatedAt = DateTime.UtcNow
            };

            db.Bookings.Add(booking);
            await db.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Allocates the stop's standing convoy (its StopShuttleDefault roster) to a
    /// newly created trip, preserving convoy order. This is what makes each stop
    /// run its own vehicle(s) — e.g. Humewood's 2-seater — instead of a single
    /// shared shuttle. Falls back to an accessible shuttle only if the stop has
    /// no configured defaults.
    /// </summary>
    private static async Task AllocateShuttleAsync(ShuttleDbContext db, TripInstance trip)
    {
        var defaults = await db.StopShuttleDefaults
            .Where(d => d.StopId == trip.StopId)
            .OrderBy(d => d.OrderPosition)
            .ToListAsync();

        if (defaults.Count > 0)
        {
            foreach (var d in defaults)
            {
                db.TripShuttleAllocations.Add(new TripShuttleAllocation
                {
                    TripInstanceId = trip.TripInstanceId,
                    ShuttleId = d.ShuttleId,
                    OrderPosition = d.OrderPosition,
                    IsOperational = true,
                    AssignedAt = DateTime.UtcNow
                });
            }

            await db.SaveChangesAsync();
            return;
        }

        // Fallback: stop has no configured convoy — assign one accessible shuttle.
        var shuttle = await db.Shuttles
            .OrderByDescending(s => s.IsAccessibleVehicle)
            .ThenBy(s => s.ShuttleId)
            .FirstOrDefaultAsync();

        if (shuttle == null) return;

        db.TripShuttleAllocations.Add(new TripShuttleAllocation
        {
            TripInstanceId = trip.TripInstanceId,
            ShuttleId = shuttle.ShuttleId,
            OrderPosition = 1,
            IsOperational = true,
            AssignedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Backfills convoy allocations for trips on a given date that have no shuttles
    /// assigned, using each trip's stop convoy (StopShuttleDefault).
    /// </summary>
    private static async Task BackfillShuttleAllocationsAsync(ShuttleDbContext db, DateOnly targetDate)
    {
        var tripsWithoutShuttles = await db.TripInstances
            .Where(t => t.TripDate == targetDate &&
                        !db.TripShuttleAllocations.Any(a => a.TripInstanceId == t.TripInstanceId))
            .ToListAsync();

        if (tripsWithoutShuttles.Count == 0) return;

        foreach (var trip in tripsWithoutShuttles)
        {
            await AllocateShuttleAsync(db, trip);
        }
    }
}
