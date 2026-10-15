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
    /// <returns>
    /// The subset of <paramref name="fcmTokens"/> that FCM reported as invalid
    /// or no longer registered. Callers should delete these from storage so the
    /// token table self-cleans. Empty when all sends succeed or Firebase is not
    /// configured.
    /// </returns>
    Task<IReadOnlyList<string>> SendToTokensAsync(
        IEnumerable<string> fcmTokens,
        string title,
        string message,
        IReadOnlyDictionary<string, string>? data = null);
}
