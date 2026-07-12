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

    public async Task SendToTokensAsync(IEnumerable<string> fcmTokens, string title, string message)
    {
        var tokens = fcmTokens.ToList();
        if (tokens.Count == 0)
            return;

        // Graceful degradation: if Firebase is not initialized, skip dispatch
        if (FirebaseApp.DefaultInstance == null)
        {
            _logger.LogWarning(
                "Firebase is not configured. Skipping FCM dispatch for {TokenCount} token(s). Title: {Title}",
                tokens.Count, title);
            return;
        }

        var multicastMessage = new MulticastMessage
        {
            Tokens = tokens,
            Notification = new Notification
            {
                Title = title,
                Body = message
            }
        };

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
                        _logger.LogWarning(
                            "FCM dispatch failed for token index {Index} (token: {Token}). Error: {Error}",
                            i,
                            tokens[i][..Math.Min(tokens[i].Length, 20)] + "...",
                            sendResponse.Exception?.Message ?? "Unknown error");
                    }
                }
            }

            _logger.LogInformation(
                "FCM multicast completed. Success: {Success}, Failures: {Failures}",
                response.SuccessCount, response.FailureCount);
        }
        catch (Exception ex)
        {
            // Never abort — log the error and continue
            _logger.LogError(ex, "FCM multicast dispatch failed entirely. Title: {Title}", title);
        }
    }
}
