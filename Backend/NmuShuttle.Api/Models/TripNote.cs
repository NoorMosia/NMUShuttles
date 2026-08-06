using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NmuShuttle.Api.Models;

/// <summary>
/// Freeform admin note attached to a TripInstance. Used to document incidents,
/// replacements, and general operational information. May trigger incident resolution
/// when created with resolveIncident = true on an IncidentActive trip.
/// </summary>
public class TripNote
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int TripNoteId { get; set; }

    [Required]
    public int TripInstanceId { get; set; }

    /// <summary>
    /// Constrained to: 'Incident', 'Replacement', 'General', 'NoShow'.
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string NoteType { get; set; } = string.Empty;

    /// <summary>
    /// Freeform body text. Minimum 1 character enforced at API layer.
    /// </summary>
    [Required]
    public string Body { get; set; } = string.Empty;

    [Required]
    public int CreatedByAdminId { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Set to true when this note triggers BreakdownLog resolution.
    /// </summary>
    [Required]
    public bool IsResolutionAction { get; set; }

    // Navigation properties
    [ForeignKey(nameof(TripInstanceId))]
    public TripInstance TripInstance { get; set; } = null!;

    [ForeignKey(nameof(CreatedByAdminId))]
    public Admin CreatedByAdmin { get; set; } = null!;
}
