using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NmuShuttle.Api.Models;

public class Stop
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int StopId { get; set; }

    [Required]
    [MaxLength(100)]
    public string StopName { get; set; } = string.Empty;

    [Required]
    [MaxLength(300)]
    public string FullAddress { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string GpsCoordinates { get; set; } = string.Empty;

    [Required]
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<ScheduleTime> ScheduleTimes { get; set; } = new List<ScheduleTime>();
    public ICollection<TripInstance> TripInstances { get; set; } = new List<TripInstance>();
    public ICollection<PrioritySchedule> PrioritySchedules { get; set; } = new List<PrioritySchedule>();
    public ICollection<Marshal> Marshals { get; set; } = new List<Marshal>();
}
