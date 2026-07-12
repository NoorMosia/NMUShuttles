using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NmuShuttle.Api.Models;

public class TripShuttleAllocation
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int AllocationId { get; set; }

    [Required]
    public int TripInstanceId { get; set; }

    [Required]
    public int ShuttleId { get; set; }

    /// <summary>
    /// 1-based front-to-back convoy position. Must be unique per TripInstance.
    /// </summary>
    [Required]
    public int OrderPosition { get; set; }

    [Required]
    public bool IsOperational { get; set; } = true;

    [Required]
    public DateTime AssignedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(TripInstanceId))]
    public TripInstance TripInstance { get; set; } = null!;

    [ForeignKey(nameof(ShuttleId))]
    public Shuttle Shuttle { get; set; } = null!;
}
