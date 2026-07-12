namespace NmuShuttle.Api.DTOs;

public class AdminLoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string Role { get; set; } = "Admin";
}
