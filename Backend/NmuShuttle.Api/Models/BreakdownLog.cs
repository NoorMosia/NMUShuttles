using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NmuShuttle.Api.Models;

public class BreakdownLog
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int BreakdownLogId { get; set; }

    [Required]
    public int TripInstanceId { get; set; }

    [Required]
    public int ShuttleId { get; set; }

    /// <summary>
    /// Denormalized from Shuttle at flag time so analytics remain stable
    /// even if the Shuttle's vendor is later updated.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string VendorCompanyName { get; set; } = string.Empty;

    [Required]
    public DateTime FlaggedAt { get; set; }

    [Required]
    public int FlaggedByMarshalId { get; set; }

    /// <summary>
    /// Set by Admin when resolving. CHECK IN ('Replaced','Cancelled').
    /// </summary>
    [MaxLength(20)]
    public string? ResolutionType { get; set; }

    /// <summary>
    /// Set when ResolutionType = 'Replaced'.
    /// </summary>
    public int? ReplacementShuttleId { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public int? ResolvedByAdminId { get; set; }

    // Navigation properties
    [ForeignKey(nameof(TripInstanceId))]
    public TripInstance TripInstance { get; set; } = null!;

    [ForeignKey(nameof(ShuttleId))]
    public Shuttle Shuttle { get; set; } = null!;

    [ForeignKey(nameof(FlaggedByMarshalId))]
    public Marshal FlaggedByMarshal { get; set; } = null!;

    [ForeignKey(nameof(ReplacementShuttleId))]
    public Shuttle? ReplacementShuttle { get; set; }

    [ForeignKey(nameof(ResolvedByAdminId))]
    public Admin? ResolvedByAdmin { get; set; }
}
