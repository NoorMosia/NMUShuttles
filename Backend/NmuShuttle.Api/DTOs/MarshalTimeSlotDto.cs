namespace NmuShuttle.Api.DTOs;

/// <summary>
/// Grouped time-slot view for campus marshals. Aggregates all trips (across stops)
/// that share the same departure time and direction into a single entry.
/// </summary>
public class MarshalTimeSlotDto
{
    public TimeOnly DepartureTime { get; set; }
    public string Direction { get; set; } = string.Empty;
    public DateOnly TripDate { get; set; }

    /// <summary>
    /// Combined status: if any trip in the slot is active, it's active.
    /// Priority: InTransit > Locked > Scheduling > Completed.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Sum of capacity across all trips in this time slot.
    /// </summary>
    public int TotalCapacity { get; set; }

    /// <summary>
    /// Sum of confirmed bookings across all trips in this time slot.
    /// </summary>
    public int ConfirmedCount { get; set; }

    /// <summary>
    /// Total available seats across all trips in this time slot.
    /// </summary>
    public int AvailableSeats { get; set; }

    /// <summary>
    /// Number of students already boarded across all trips in this time slot.
    /// </summary>
    public int BoardedCount { get; set; }

    /// <summary>
    /// Number of distinct stops served at this time slot.
    /// </summary>
    public int StopCount { get; set; }

    /// <summary>
    /// Whether scanning is open (within 35-min window).
    /// </summary>
    public bool IsScanningOpen { get; set; }

    /// <summary>
    /// Individual trip IDs that make up this time slot (for drill-down).
    /// </summary>
    public List<int> TripInstanceIds { get; set; } = new();
}
