namespace NmuShuttle.Api.Services;

/// <summary>
/// High-level notification service that creates DB records and dispatches push notifications.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Send a notification to a single student (creates DB record + dispatches FCM).
    /// </summary>
    Task DispatchAsync(int studentId, string title, string message, int? tripInstanceId = null);

    /// <summary>
    /// Send a notification to ALL students with confirmed bookings on a trip.
    /// </summary>
    Task DispatchToTripPassengersAsync(int tripInstanceId, string title, string message);
}
