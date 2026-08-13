namespace NmuShuttle.Api.DTOs;

/// <summary>
/// Passenger list for a time slot, used by marshals for manual check-in fallback.
/// </summary>
public class MarshalPassengerListDto
{
    public TimeOnly DepartureTime { get; set; }
    public string Direction { get; set; } = string.Empty;
    public DateOnly TripDate { get; set; }
    public int TotalConfirmed { get; set; }
    public int BoardedCount { get; set; }
    public int PendingCount { get; set; }
    public List<MarshalPassengerDto> Passengers { get; set; } = new();
}

/// <summary>
/// Individual passenger entry in the marshal's time-slot passenger list.
/// </summary>
public class MarshalPassengerDto
{
    public int BookingId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentNumber { get; set; } = string.Empty;

    /// <summary>
    /// The stop this student is headed to (or coming from). Informational only.
    /// </summary>
    public string StopName { get; set; } = string.Empty;

    public string BoardedStatus { get; set; } = string.Empty;
    public int? ShuttleId { get; set; }
    public DateTime? BoardedAt { get; set; }
}
