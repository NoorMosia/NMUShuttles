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

        var sastToday = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(2));

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
    /// Allocates a shuttle to a newly created trip using round-robin from available shuttles.
    /// Prefers accessible vehicles at OrderPosition 1.
    /// </summary>
    private static async Task AllocateShuttleAsync(ShuttleDbContext db, TripInstance trip)
    {
        // Pick an accessible shuttle first, then any shuttle
        var shuttle = await db.Shuttles
            .OrderByDescending(s => s.IsAccessibleVehicle)
            .ThenBy(s => s.ShuttleId)
            .FirstOrDefaultAsync();

        if (shuttle == null) return;

        var allocation = new TripShuttleAllocation
        {
            TripInstanceId = trip.TripInstanceId,
            ShuttleId = shuttle.ShuttleId,
            OrderPosition = 1,
            IsOperational = true,
            AssignedAt = DateTime.UtcNow
        };

        db.TripShuttleAllocations.Add(allocation);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Backfills shuttle allocations for trips on a given date that have no shuttles assigned.
    /// </summary>
    private static async Task BackfillShuttleAllocationsAsync(ShuttleDbContext db, DateOnly targetDate)
    {
        var tripsWithoutShuttles = await db.TripInstances
            .Where(t => t.TripDate == targetDate &&
                        !db.TripShuttleAllocations.Any(a => a.TripInstanceId == t.TripInstanceId))
            .ToListAsync();

        if (!tripsWithoutShuttles.Any()) return;

        var shuttle = await db.Shuttles
            .OrderByDescending(s => s.IsAccessibleVehicle)
            .ThenBy(s => s.ShuttleId)
            .FirstOrDefaultAsync();

        if (shuttle == null) return;

        foreach (var trip in tripsWithoutShuttles)
        {
            var allocation = new TripShuttleAllocation
            {
                TripInstanceId = trip.TripInstanceId,
                ShuttleId = shuttle.ShuttleId,
                OrderPosition = 1,
                IsOperational = true,
                AssignedAt = DateTime.UtcNow
            };

            db.TripShuttleAllocations.Add(allocation);
        }

        await db.SaveChangesAsync();
    }
}
