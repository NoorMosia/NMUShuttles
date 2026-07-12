using System.ComponentModel.DataAnnotations;

namespace NmuShuttle.Api.DTOs;

/// <summary>
/// Request body for resolving a breakdown incident.
/// </summary>
public class ResolveBreakdownDto
{
    /// <summary>
    /// Resolution type: "Replaced" or "Cancelled".
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string ResolutionType { get; set; } = string.Empty;

    /// <summary>
    /// Required when ResolutionType is "Replaced". The shuttle to assign as replacement.
    /// </summary>
    public int? ReplacementShuttleId { get; set; }
}
