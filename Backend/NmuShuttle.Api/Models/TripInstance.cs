using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NmuShuttle.Api.Models;

public class TripInstance
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int TripInstanceId { get; set; }

    [Required]
    public int ScheduleTimeId { get; set; }

    [Required]
    public int StopId { get; set; }

    [Required]
    public DateOnly TripDate { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Scheduling";

    public DateTime? LockedAt { get; set; }

    public DateTime? InTransitAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(ScheduleTimeId))]
    public ScheduleTime ScheduleTime { get; set; } = null!;

    [ForeignKey(nameof(StopId))]
    public Stop Stop { get; set; } = null!;

    public ICollection<TripShuttleAllocation> TripShuttleAllocations { get; set; } = new List<TripShuttleAllocation>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<Report> Reports { get; set; } = new List<Report>();
    public ICollection<BreakdownLog> BreakdownLogs { get; set; } = new List<BreakdownLog>();
}
