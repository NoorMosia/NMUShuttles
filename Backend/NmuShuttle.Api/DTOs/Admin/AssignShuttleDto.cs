using System.ComponentModel.DataAnnotations;

namespace NmuShuttle.Api.DTOs.Admin;

/// <summary>
/// Request body for assigning a shuttle to a trip instance.
/// </summary>
public class AssignShuttleDto
{
    [Required]
    public int ShuttleId { get; set; }
}
