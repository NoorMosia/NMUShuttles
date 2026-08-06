using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NmuShuttle.Api.Models;

/// <summary>
/// Defines the standing default convoy roster for a Stop.
/// Shuttles linked here are automatically allocated to TripInstances
/// when the Rolling Injection Job creates trips for this Stop.
/// </summary>
public class StopShuttleDefault
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int StopShuttleDefaultId { get; set; }

    [Required]
    public int StopId { get; set; }

    [Required]
    public int ShuttleId { get; set; }

    /// <summary>
    /// 1-based convoy position. Accessible shuttles always get the lowest positions.
    /// Must be unique and contiguous per Stop.
    /// </summary>
    [Required]
    public int OrderPosition { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(StopId))]
    public Stop Stop { get; set; } = null!;

    [ForeignKey(nameof(ShuttleId))]
    public Shuttle Shuttle { get; set; } = null!;
}
