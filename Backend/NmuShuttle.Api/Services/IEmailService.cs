namespace NmuShuttle.Api.Services;

/// <summary>
/// Sends transactional email. Provider-agnostic abstraction so the OTP flow
/// does not depend on any specific email vendor.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends a one-time passcode to the given recipient.
    /// Implementations should throw on hard failures so callers can log/handle.
    /// </summary>
    /// <param name="toEmail">Recipient email address.</param>
    /// <param name="otp">The 5-digit one-time passcode.</param>
    /// <param name="recipientName">Optional display name for a friendlier greeting.</param>
    Task SendOtpAsync(string toEmail, string otp, string? recipientName = null);
}
