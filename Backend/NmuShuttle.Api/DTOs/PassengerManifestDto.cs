namespace NmuShuttle.Api.DTOs;

public class PassengerManifestDto
{
    public int TripInstanceId { get; set; }
    public int TotalConfirmed { get; set; }
    public int BoardedCount { get; set; }
    public int PendingCount { get; set; }
    public List<PassengerDto> Passengers { get; set; } = new();
}

public class PassengerDto
{
    public int BookingId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentNumber { get; set; } = string.Empty;
    public string BoardedStatus { get; set; } = string.Empty;
    public int? ShuttleId { get; set; }
    public DateTime? BoardedAt { get; set; }
}
