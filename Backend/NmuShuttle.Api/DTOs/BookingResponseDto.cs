namespace NmuShuttle.Api.DTOs;

public class BookingResponseDto
{
    public int BookingId { get; set; }
    public string BookingStatus { get; set; } = string.Empty;
    public string BookingType { get; set; } = string.Empty;
    public string QrCodeToken { get; set; } = string.Empty;
    public DateOnly TripDate { get; set; }
    public TimeOnly DepartureTime { get; set; }
    public string Direction { get; set; } = string.Empty;
    public string StopName { get; set; } = string.Empty;
    public string CampusName { get; set; } = string.Empty;
    public string BoardedStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
