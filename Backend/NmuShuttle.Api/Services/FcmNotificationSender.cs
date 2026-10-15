using FirebaseAdmin;
using FirebaseAdmin.Messaging;

namespace NmuShuttle.Api.Services;

/// <summary>
/// FCM sender implementation using FirebaseAdmin SDK.
/// Gracefully degrades if Firebase is not configured (development environments).
/// </summary>
public class FcmNotificationSender : IFcmSender
{
    private readonly ILogger<FcmNotificationSender> _logger;

    public FcmNotificationSender(ILogger<FcmNotificationSender> logger)
    {
        _logger = logger;
    }

    public async Task<IReadOnlyList<string>> SendToTokensAsync(
        IEnumerable<string> fcmTokens,
        string title,
        string message,
        IReadOnlyDictionary<string, string>? data = null)
    {
        var tokens = fcmTokens.ToList();
        if (tokens.Count == 0)
            return Array.Empty<string>();

        // Graceful degradation: if Firebase is not initialized, skip dispatch
        if (FirebaseApp.DefaultInstance == null)
        {
            _logger.LogWarning(
                "Firebase is not configured. Skipping FCM dispatch for {TokenCount} token(s). Title: {Title}",
                tokens.Count, title);
            return Array.Empty<string>();
        }

        var multicastMessage = new MulticastMessage
        {
            Tokens = tokens,
            Notification = new Notification
            {
                Title = title,
                Body = message
            },
            // Data payload travels with the notification so the client can
            // deep-link when the user taps it (foreground, background, or
            // cold-start). FCM requires all data values to be strings.
            Data = data?.ToDictionary(kv => kv.Key, kv => kv.Value),
            // Android-specific delivery config. Without high priority and an
            // explicit channel id, Android 8+ delivers the notification at
            // normal priority and it lands silently in the drawer instead of
            // appearing as a heads-up banner. The channel id must match the one
            // created in the Flutter PushNotificationService (and the
            // default_notification_channel_id in AndroidManifest.xml).
            Android = new AndroidConfig
            {
                Priority = Priority.High,
                Notification = new AndroidNotification
                {
                    ChannelId = "nmu_shuttle_default"
                }
            }
        };

        var invalidTokens = new List<string>();

        try
        {
            var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(multicastMessage);

            if (response.FailureCount > 0)
            {
                for (int i = 0; i < response.Responses.Count; i++)
                {
                    var sendResponse = response.Responses[i];
                    if (!sendResponse.IsSuccess)
                    {
                        var messagingError =
                            (sendResponse.Exception as FirebaseMessagingException)?.MessagingErrorCode;

                        // A token FCM rejects as unregistered or malformed will
                        // never succeed again — flag it for deletion so the
                        // token table self-cleans. Other errors (e.g. transient
                        // server/quota issues) are left in place for retry.
                        if (messagingError is MessagingErrorCode.Unregistered
                            or MessagingErrorCode.InvalidArgument
                            or MessagingErrorCode.SenderIdMismatch)
                        {
                            invalidTokens.Add(tokens[i]);
                        }

                        _logger.LogWarning(
                            "FCM dispatch failed for token index {Index} (token: {Token}). Error: {Error}",
                            i,
                            tokens[i][..Math.Min(tokens[i].Length, 20)] + "...",
                            sendResponse.Exception?.Message ?? "Unknown error");
                    }
                }
            }

            _logger.LogInformation(
                "FCM multicast completed. Success: {Success}, Failures: {Failures}, Invalid: {Invalid}",
                response.SuccessCount, response.FailureCount, invalidTokens.Count);
        }
        catch (Exception ex)
        {
            // Never abort — log the error and continue
            _logger.LogError(ex, "FCM multicast dispatch failed entirely. Title: {Title}", title);
        }

        return invalidTokens;
    }
}
