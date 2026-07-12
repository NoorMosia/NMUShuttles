namespace NmuShuttle.Api.DTOs;

/// <summary>
/// Response body for successful OTP verification containing a JWT.
/// </summary>
public record AuthResponseDto(string Token, DateTime ExpiresAt, string Role);
