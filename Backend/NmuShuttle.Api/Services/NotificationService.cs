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

    public async Task DispatchAsync(int studentId, string title, string message, int? tripInstanceId = null)
    {
        // Always create a Notification DB record
        var notification = new Notification
        {
            StudentId = studentId,
            Title = title,
            Message = message,
            RelatedTripInstanceId = tripInstanceId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync();

        // Look up all device tokens for this student
        var tokens = await _db.DeviceRegistrations
            .Where(d => d.OwnerId == studentId && d.OwnerType == "Student")
            .Select(d => d.FcmToken)
            .ToListAsync();

        if (tokens.Count > 0)
        {
            await _fcmSender.SendToTokensAsync(tokens, title, message);
        }
        else
        {
            _logger.LogDebug(
                "No device tokens found for student {StudentId}. Notification saved to DB only.",
                studentId);
        }
    }

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
            await DispatchAsync(studentId, title, message, tripInstanceId);
        }
    }
}
