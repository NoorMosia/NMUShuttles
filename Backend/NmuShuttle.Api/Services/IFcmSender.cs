namespace NmuShuttle.Api.Services;

/// <summary>
/// Low-level abstraction over Firebase Cloud Messaging multicast dispatch.
/// </summary>
public interface IFcmSender
{
    /// <summary>
    /// Send a push notification to the given FCM device tokens.
    /// Partial failures are logged but never abort the dispatch.
    /// </summary>
    /// <param name="data">
    /// Optional key/value payload delivered alongside the notification. Used by
    /// the client to deep-link on tap (e.g. "tripInstanceId", "type").
    /// </param>
    Task SendToTokensAsync(
        IEnumerable<string> fcmTokens,
        string title,
        string message,
        IReadOnlyDictionary<string, string>? data = null);
}
