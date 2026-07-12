using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NmuShuttle.Api.Models;

public class PrioritySchedule
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PriorityScheduleId { get; set; }

    [Required]
    public int StudentId { get; set; }

    [Required]
    public int StopId { get; set; }

    [Required]
    public int ScheduleTimeId { get; set; }

    [Required]
    [MaxLength(10)]
    public string DayOfWeek { get; set; } = string.Empty;

    [Required]
    public bool IsActive { get; set; } = true;

    [Required]
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(StudentId))]
    public Student Student { get; set; } = null!;

    [ForeignKey(nameof(StopId))]
    public Stop Stop { get; set; } = null!;

    [ForeignKey(nameof(ScheduleTimeId))]
    public ScheduleTime ScheduleTime { get; set; } = null!;
}
