using System.ComponentModel.DataAnnotations;

namespace NmuShuttle.Api.DTOs;

public class AdminOtpVerifyDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(5, MinimumLength = 5)]
    public string Otp { get; set; } = string.Empty;
}
