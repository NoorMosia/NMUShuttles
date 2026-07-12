using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NmuShuttle.Api.Models;

public class ScheduleTime
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ScheduleTimeId { get; set; }

    [Required]
    public TimeOnly DepartureTime { get; set; }

    [Required]
    [MaxLength(20)]
    public string Direction { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string ScheduleType { get; set; } = string.Empty;

    [Required]
    public int CampusId { get; set; }

    [Required]
    public int StopId { get; set; }

    // Navigation properties
    [ForeignKey(nameof(CampusId))]
    public Campus Campus { get; set; } = null!;

    [ForeignKey(nameof(StopId))]
    public Stop Stop { get; set; } = null!;

    public ICollection<TripInstance> TripInstances { get; set; } = new List<TripInstance>();
    public ICollection<PrioritySchedule> PrioritySchedules { get; set; } = new List<PrioritySchedule>();
}
