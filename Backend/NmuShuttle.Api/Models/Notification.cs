using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NmuShuttle.Api.Models;

public class Notification
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int NotificationId { get; set; }

    /// <summary>
    /// The StudentId or MarshalId depending on <see cref="OwnerType"/>.
    /// </summary>
    [Required]
    public int OwnerId { get; set; }

    /// <summary>
    /// "Student" or "Marshal". Determines which user the notification belongs to.
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string OwnerType { get; set; } = "Student";

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;

    [Required]
    public bool IsRead { get; set; }

    public int? RelatedTripInstanceId { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(RelatedTripInstanceId))]
    public TripInstance? RelatedTripInstance { get; set; }
}
