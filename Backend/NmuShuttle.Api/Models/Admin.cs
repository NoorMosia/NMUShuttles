using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NmuShuttle.Api.Models;

public class Admin
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int AdminId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string StaffNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;

    [MaxLength(128)]
    public string? OtpHash { get; set; }

    public DateTime? OtpExpiry { get; set; }

    // Navigation properties
    public ICollection<BreakdownLog> ResolvedBreakdownLogs { get; set; } = new List<BreakdownLog>();
    public ICollection<TripNote> TripNotes { get; set; } = new List<TripNote>();
}
