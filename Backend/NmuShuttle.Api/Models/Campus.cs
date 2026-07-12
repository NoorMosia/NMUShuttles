using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NmuShuttle.Api.Models;

public class Campus
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CampusId { get; set; }

    [Required]
    [MaxLength(100)]
    public string CampusName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string GpsCoordinates { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<ScheduleTime> ScheduleTimes { get; set; } = new List<ScheduleTime>();
    public ICollection<Marshal> Marshals { get; set; } = new List<Marshal>();
}
