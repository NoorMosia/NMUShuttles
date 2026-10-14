using Microsoft.EntityFrameworkCore;
using NmuShuttle.Api.Models;
using NmuShuttle.Api.Services;

namespace NmuShuttle.Api.Data;

/// <summary>
/// Seeds a trip at full capacity with 2 waitlisted students, then immediately
/// locks the trip and runs waitlist promotion — triggering "Waitlist Removed"
/// notifications for Students 1 and 2 on startup.
///
/// This provides an instant demo of the notification flow without waiting
/// for the LockoutGateService timer.
///
/// Idempotent: only runs if no "Waitlist Removed" notifications exist yet.
/// </summary>
public class DemoWaitlistSeeder
{
    private readonly ShuttleDbContext _db;
    private readonly IWaitlistPromotionService _waitlistService;
    private readonly ILogger<DemoWaitlistSeeder> _logger;

    // Waitlisted students — both have DeviceRegistrations in the static seed.
    private static readonly int[] WaitlistedStudentIds = { 1, 2 };

    // Filler students to fill capacity.
    private static readonly int[] FillerStudentIds =
    {
        3, 4, 5, 6, 7, 8, 9, 10, 13, 16, 20, 25, 30, 35, 40, 45, 55,
        11, 12, 14, 15, 17, 18, 19, 21, 22, 23, 24, 26, 27, 28, 29
    };

    public DemoWaitlistSeeder(
        ShuttleDbContext db,
        IWaitlistPromotionService waitlistService,
        ILogger<DemoWaitlistSeeder> logger)
    {
        _db = db;
        _waitlistService = waitlistService;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        // Idempotent guard: skip if we've already generated waitlist-removed notifications
        var alreadyRan = await _db.Notifications
            .AnyAsync(n => n.Title == "Waitlist Removed");

        if (alreadyRan)
        {
            _logger.LogInformation("DemoWaitlistSeeder: Already ran (Waitlist Removed notifications exist). Skipping.");
            return;
        }

        // Find a future trip in "Scheduling" status to use for the demo
        var utcNow = DateTime.UtcNow;

        var targetTrip = await _db.TripInstances
            .Include(t => t.ScheduleTime)
            .Where(t => t.Status == "Scheduling")
            .OrderBy(t => t.TripDate)
            .ThenBy(t => t.ScheduleTime.DepartureTime)
            .FirstOrDefaultAsync();

        if (targetTrip == null)
        {
            _logger.LogWarning("DemoWaitlistSeeder: No Scheduling trips found. Skipping.");
            return;
        }

        // Get the trip's total capacity
        var totalCapacity = await _db.TripShuttleAllocations
            .Where(a => a.TripInstanceId == targetTrip.TripInstanceId && a.IsOperational)
            .Join(_db.Shuttles, a => a.ShuttleId, s => s.ShuttleId, (a, s) => s.Capacity)
            .SumAsync(c => c);

        if (totalCapacity == 0)
        {
            _logger.LogWarning(
                "DemoWaitlistSeeder: Trip {TripId} has 0 capacity. Skipping.",
                targetTrip.TripInstanceId);
            return;
        }

        _logger.LogInformation(
            "DemoWaitlistSeeder: Using Trip {TripId} (Stop={Stop}, Date={Date}, Departure={Time} SAST, Capacity={Cap}).",
            targetTrip.TripInstanceId, targetTrip.StopId, targetTrip.TripDate,
            targetTrip.ScheduleTime.DepartureTime, totalCapacity);

        // Remove any existing bookings for this trip (e.g., priority injections from StartupTripGenerator)
        var existingBookings = await _db.Bookings
            .Where(b => b.TripInstanceId == targetTrip.TripInstanceId)
            .ToListAsync();
        if (existingBookings.Count > 0)
        {
            _db.Bookings.RemoveRange(existingBookings);
            await _db.SaveChangesAsync();
        }

        // Fill to capacity with Confirmed bookings
        var fillerCount = Math.Min(totalCapacity, FillerStudentIds.Length);
        for (var i = 0; i < fillerCount; i++)
        {
            _db.Bookings.Add(new Booking
            {
                StudentId = FillerStudentIds[i],
                TripInstanceId = targetTrip.TripInstanceId,
                BookingStatus = "Confirmed",
                BookingType = "Standard",
                BoardedStatus = "Pending",
                QrCodeToken = $"demo_wl_{targetTrip.TripInstanceId}_s{FillerStudentIds[i]}_{Guid.NewGuid():N}",
                CreatedAt = utcNow.AddHours(-2)
            });
        }
        await _db.SaveChangesAsync();

        // Add waitlisted bookings for Students 1 and 2
        foreach (var studentId in WaitlistedStudentIds)
        {
            _db.Bookings.Add(new Booking
            {
                StudentId = studentId,
                TripInstanceId = targetTrip.TripInstanceId,
                BookingStatus = "WaitingList",
                BookingType = "Standard",
                BoardedStatus = "Pending",
                QrCodeToken = $"demo_wl_{targetTrip.TripInstanceId}_s{studentId}_waitlist_{Guid.NewGuid():N}",
                CreatedAt = utcNow.AddHours(-1)
            });
        }
        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "DemoWaitlistSeeder: Trip {TripId} filled with {Cap} confirmed + {Wl} waitlisted bookings.",
            targetTrip.TripInstanceId, fillerCount, WaitlistedStudentIds.Length);

        // Lock the trip directly
        targetTrip.Status = "Locked";
        targetTrip.LockedAt = utcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "DemoWaitlistSeeder: Trip {TripId} locked. Running waitlist promotion...",
            targetTrip.TripInstanceId);

        // Run waitlist promotion — this will find 0 available slots and notify unpromoted students
        await _waitlistService.RunAsync(targetTrip.TripInstanceId);

        _logger.LogInformation(
            "DemoWaitlistSeeder: DONE. Students {S1} and {S2} should now have 'Waitlist Removed' notifications.",
            WaitlistedStudentIds[0], WaitlistedStudentIds[1]);
    }
}
