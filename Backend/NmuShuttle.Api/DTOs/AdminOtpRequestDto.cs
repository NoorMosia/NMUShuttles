using System.ComponentModel.DataAnnotations;

namespace NmuShuttle.Api.DTOs;

public class AdminOtpRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
