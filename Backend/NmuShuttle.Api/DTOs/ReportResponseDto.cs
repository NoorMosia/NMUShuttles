namespace NmuShuttle.Api.DTOs;

public class ReportResponseDto
{
    public int ReportId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? AdminNotes { get; set; }
    public int? TripInstanceId { get; set; }
    public int? BookingId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
