using Microsoft.EntityFrameworkCore;
using NmuShuttle.Api.Data;
using NmuShuttle.Api.Models;

namespace NmuShuttle.Api.Services;

/// <summary>
/// Notification service that always persists a DB record and optionally dispatches FCM.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly ShuttleDbContext _db;
    private readonly IFcmSender _fcmSender;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        ShuttleDbContext db,
        IFcmSender fcmSender,
        ILogger<NotificationService> logger)
    {
        _db = db;
        _fcmSender = fcmSender;
        _logger = logger;
    }

    public Task DispatchAsync(int studentId, string title, string message, int? tripInstanceId = null)
        => DispatchToOwnerAsync(studentId, "Student", title, message, tripInstanceId);

    public Task DispatchToMarshalAsync(int marshalId, string title, string message, int? tripInstanceId = null)
        => DispatchToOwnerAsync(marshalId, "Marshal", title, message, tripInstanceId);

    public async Task DispatchToTripPassengersAsync(int tripInstanceId, string title, string message)
    {
        // Find all confirmed bookings for the trip
        var studentIds = await _db.Bookings
            .Where(b => b.TripInstanceId == tripInstanceId && b.BookingStatus == "Confirmed")
            .Select(b => b.StudentId)
            .Distinct()
            .ToListAsync();

        _logger.LogInformation(
            "Dispatching notification to {Count} student(s) for TripInstance {TripInstanceId}. Title: {Title}",
            studentIds.Count, tripInstanceId, title);

        // Dispatch to each student (creates individual DB records + FCM)
        foreach (var studentId in studentIds)
        {
            await DispatchToOwnerAsync(studentId, "Student", title, message, tripInstanceId);
        }
    }

    public async Task DispatchToAllMarshalsAsync(string title, string message, int? tripInstanceId = null)
    {
        var marshalIds = await _db.Marshals
            .Where(m => m.IsActive)
            .Select(m => m.MarshalId)
            .ToListAsync();

        _logger.LogInformation(
            "Dispatching broadcast notification to {Count} active marshal(s). Title: {Title}",
            marshalIds.Count, title);

        foreach (var marshalId in marshalIds)
        {
            await DispatchToOwnerAsync(marshalId, "Marshal", title, message, tripInstanceId);
        }
    }

    /// <summary>
    /// Core dispatch: always persists a Notification row for the owner, then
    /// pushes to that owner's registered device tokens (if any).
    /// </summary>
    private async Task DispatchToOwnerAsync(
        int ownerId, string ownerType, string title, string message, int? tripInstanceId)
    {
        // Always create a Notification DB record
        var notification = new Notification
        {
            OwnerId = ownerId,
            OwnerType = ownerType,
            Title = title,
            Message = message,
            RelatedTripInstanceId = tripInstanceId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync();

        // Look up all device tokens for this owner
        var tokens = await _db.DeviceRegistrations
            .Where(d => d.OwnerId == ownerId && d.OwnerType == ownerType)
            .Select(d => d.FcmToken)
            .ToListAsync();

        if (tokens.Count > 0)
        {
            // Carry routing hints so the app can deep-link when the user taps
            // the notification. Values must be strings for FCM.
            var data = new Dictionary<string, string>
            {
                ["notificationId"] = notification.NotificationId.ToString(),
                ["ownerType"] = ownerType,
            };
            if (tripInstanceId.HasValue)
            {
                data["tripInstanceId"] = tripInstanceId.Value.ToString();
            }

            await _fcmSender.SendToTokensAsync(tokens, title, message, data);
        }
        else
        {
            _logger.LogDebug(
                "No device tokens found for {OwnerType} {OwnerId}. Notification saved to DB only.",
                ownerType, ownerId);
        }
    }
}
