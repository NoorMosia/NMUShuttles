namespace NmuShuttle.Api.DTOs;

public class TripAvailabilityDto
{
    public int TripInstanceId { get; set; }
    public TimeOnly DepartureTime { get; set; }
    public string Direction { get; set; } = string.Empty;
    public string StopName { get; set; } = string.Empty;
    public string CampusName { get; set; } = string.Empty;
    public DateOnly TripDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int TotalCapacity { get; set; }
    public int ConfirmedCount { get; set; }
    public int AvailableSeats { get; set; }
}
