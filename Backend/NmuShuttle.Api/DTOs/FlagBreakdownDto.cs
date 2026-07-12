using System.ComponentModel.DataAnnotations;

namespace NmuShuttle.Api.DTOs;

/// <summary>
/// Request body for flagging a shuttle breakdown.
/// </summary>
public class FlagBreakdownDto
{
    /// <summary>
    /// The ID of the shuttle that has broken down.
    /// </summary>
    [Required]
    public int ShuttleId { get; set; }
}
