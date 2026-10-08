using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace NmuShuttle.Api.Services;

/// <summary>
/// Resend (https://resend.com) implementation of <see cref="IEmailService"/>.
/// Calls the Resend REST API via a typed HttpClient. No official .NET SDK is required.
/// Configuration is read from the "Email:Resend" section (ApiKey, FromAddress, FromName).
/// </summary>
public class ResendEmailService : IEmailService
{
    private const string SendEndpoint = "https://api.resend.com/emails";

    private readonly HttpClient _httpClient;
    private readonly ILogger<ResendEmailService> _logger;
    private readonly string _fromAddress;
    private readonly string _fromName;

    public ResendEmailService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<ResendEmailService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        var section = configuration.GetSection("Email:Resend");
        var apiKey = section["ApiKey"]
            ?? throw new InvalidOperationException(
                "Resend API key is not configured. Set Email:Resend:ApiKey (env RESEND_API_KEY).");

        _fromAddress = section["FromAddress"]
            ?? throw new InvalidOperationException(
                "Resend sender address is not configured. Set Email:Resend:FromAddress (env RESEND_FROM_ADDRESS).");
        _fromName = section["FromName"] ?? "NMU Shuttle";

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);
    }

    /// <inheritdoc />
    public async Task SendOtpAsync(string toEmail, string otp, string? recipientName = null)
    {
        var greeting = string.IsNullOrWhiteSpace(recipientName) ? "Hello" : $"Hello {recipientName}";

        var payload = new
        {
            from = $"{_fromName} <{_fromAddress}>",
            to = new[] { toEmail },
            subject = "Your NMU Shuttle verification code",
            text =
                $"{greeting},\n\n" +
                $"Your NMU Shuttle verification code is: {otp}\n\n" +
                "This code expires in 5 minutes. If you did not request it, you can ignore this email.",
            html =
                $"<p>{greeting},</p>" +
                $"<p>Your NMU Shuttle verification code is:</p>" +
                $"<p style=\"font-size:28px;font-weight:bold;letter-spacing:4px;\">{otp}</p>" +
                "<p>This code expires in 5 minutes. If you did not request it, you can ignore this email.</p>"
        };

        using var response = await _httpClient.PostAsJsonAsync(SendEndpoint, payload);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            _logger.LogError(
                "Resend send failed with status {StatusCode}: {Body}",
                (int)response.StatusCode, body);
            throw new InvalidOperationException(
                $"Failed to send OTP email via Resend (HTTP {(int)response.StatusCode}).");
        }
    }
}
