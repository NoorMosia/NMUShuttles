using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NmuShuttle.Api.Models;

public class Marshal
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MarshalId { get; set; }

    [Required]
    [MaxLength(20)]
    public string StaffNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string UniversityEmail { get; set; } = string.Empty;

    /// <summary>
    /// FK to Campuses. Nullable. Marshal can have both campus and stop assignments.
    /// </summary>
    public int? AssignedCampusId { get; set; }

    /// <summary>
    /// FK to Stops. Nullable. Marshal can have both campus and stop assignments.
    /// </summary>
    public int? AssignedStopId { get; set; }

    [MaxLength(256)]
    public string? OtpHash { get; set; }

    public DateTime? OtpExpiry { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;

    [Required]
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(AssignedCampusId))]
    public Campus? AssignedCampus { get; set; }

    [ForeignKey(nameof(AssignedStopId))]
    public Stop? AssignedStop { get; set; }

    public ICollection<BreakdownLog> FlaggedBreakdownLogs { get; set; } = new List<BreakdownLog>();
}
