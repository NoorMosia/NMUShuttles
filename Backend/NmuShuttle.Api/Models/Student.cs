using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NmuShuttle.Api.Models;

public class Student
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int StudentId { get; set; }

    [Required]
    [MaxLength(20)]
    public string StudentNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string UniversityEmail { get; set; } = string.Empty;

    [Required]
    public bool IsPriorityUser { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;

    [MaxLength(256)]
    public string? OtpHash { get; set; }

    public DateTime? OtpExpiry { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<PrioritySchedule> PrioritySchedules { get; set; } = new List<PrioritySchedule>();
    public ICollection<Report> Reports { get; set; } = new List<Report>();
}
