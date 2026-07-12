using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NmuShuttle.Api.Models;

public class Booking
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int BookingId { get; set; }

    [Required]
    public int StudentId { get; set; }

    [Required]
    public int TripInstanceId { get; set; }

    [Required]
    [MaxLength(20)]
    public string BookingStatus { get; set; } = "Confirmed";

    [Required]
    [MaxLength(20)]
    public string BookingType { get; set; } = "Standard";

    [Required]
    [MaxLength(20)]
    public string BoardedStatus { get; set; } = "Pending";

    [Required]
    [MaxLength(128)]
    public string QrCodeToken { get; set; } = string.Empty;

    /// <summary>
    /// Nullable FK to Shuttles. Set at check-in or priority pre-assignment.
    /// </summary>
    public int? ShuttleId { get; set; }

    public DateTime? BoardedAt { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(StudentId))]
    public Student Student { get; set; } = null!;

    [ForeignKey(nameof(TripInstanceId))]
    public TripInstance TripInstance { get; set; } = null!;

    [ForeignKey(nameof(ShuttleId))]
    public Shuttle? Shuttle { get; set; }
}
