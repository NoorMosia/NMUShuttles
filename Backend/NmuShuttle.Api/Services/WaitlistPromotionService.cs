using Microsoft.EntityFrameworkCore;
using NmuShuttle.Api.Data;

namespace NmuShuttle.Api.Services;

/// <summary>
/// Promotes waitlisted bookings to confirmed in FIFO order (CreatedAt ASC)
/// up to the available seat capacity for a trip instance.
/// Each promoted booking triggers an FCM push notification.
/// </summary>
public class WaitlistPromotionService : IWaitlistPromotionService
{
    private readonly ShuttleDbContext _db;
    private readonly ICapacityService _capacityService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<WaitlistPromotionService> _logger;

    public WaitlistPromotionService(
        ShuttleDbContext db,
        ICapacityService capacityService,
        INotificationService notificationService,
        ILogger<WaitlistPromotionService> logger)
    {
        _db = db;
        _capacityService = capacityService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task RunAsync(int tripInstanceId)
    {
        _logger.LogInformation(
            "WaitlistPromotionService running for TripInstanceId={TripInstanceId}.", tripInstanceId);

        // Calculate available slots
        var totalCapacity = await _capacityService.GetTotalCapacityAsync(tripInstanceId);
        var confirmedCount = await _capacityService.GetConfirmedCountAsync(tripInstanceId);
        var availableSlots = totalCapacity - confirmedCount;

        if (availableSlots <= 0)
        {
            _logger.LogInformation(
                "No available slots for TripInstanceId={TripInstanceId}. Skipping promotion.", tripInstanceId);
            return;
        }

        _logger.LogInformation(
            "TripInstanceId={TripInstanceId}: {Available} slots available (capacity={Total}, confirmed={Confirmed}).",
            tripInstanceId, availableSlots, totalCapacity, confirmedCount);

        // Get waitlisted bookings in FIFO order
        var waitlistedBookings = await _db.Bookings
            .Where(b => b.TripInstanceId == tripInstanceId && b.BookingStatus == "WaitingList")
            .OrderBy(b => b.CreatedAt)
            .Take(availableSlots)
            .ToListAsync();

        if (waitlistedBookings.Count == 0)
        {
            _logger.LogInformation(
                "No waitlisted bookings found for TripInstanceId={TripInstanceId}.", tripInstanceId);
            return;
        }

        _logger.LogInformation(
            "Promoting {Count} waitlisted bookings for TripInstanceId={TripInstanceId}.",
            waitlistedBookings.Count, tripInstanceId);

        // Promote each booking and dispatch notification
        foreach (var booking in waitlistedBookings)
        {
            booking.BookingStatus = "Confirmed";

            await _notificationService.DispatchAsync(
                booking.StudentId,
                "Booking Confirmed",
                "Your waitlisted booking has been confirmed!",
                tripInstanceId);
        }

        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "WaitlistPromotionService completed. Promoted {Count} bookings for TripInstanceId={TripInstanceId}.",
            waitlistedBookings.Count, tripInstanceId);
    }
}
