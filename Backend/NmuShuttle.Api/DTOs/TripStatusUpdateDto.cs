using System.ComponentModel.DataAnnotations;

namespace NmuShuttle.Api.DTOs;

/// <summary>
/// Request body for updating a trip instance's status.
/// </summary>
public class TripStatusUpdateDto
{
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = string.Empty;
}
