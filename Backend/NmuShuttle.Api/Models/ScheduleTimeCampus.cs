using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NmuShuttle.Api.Models;

/// <summary>
/// Junction table mapping a ScheduleTime to the ordered sequence of Campuses
/// the shuttle visits on that route. Replaces the single CampusId FK on ScheduleTime.
/// </summary>
public class ScheduleTimeCampus
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ScheduleTimeCampusId { get; set; }

    [Required]
    public int ScheduleTimeId { get; set; }

    [Required]
    public int CampusId { get; set; }

    /// <summary>
    /// 1-based physical sequence along the route (e.g., 1 = first campus stop).
    /// Must be unique and contiguous per ScheduleTime.
    /// </summary>
    [Required]
    public int StopOrder { get; set; }

    // Navigation properties
    [ForeignKey(nameof(ScheduleTimeId))]
    public ScheduleTime ScheduleTime { get; set; } = null!;

    [ForeignKey(nameof(CampusId))]
    public Campus Campus { get; set; } = null!;
}
