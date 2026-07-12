namespace NmuShuttle.Api.DTOs;

public class CreateReportDto
{
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? TripInstanceId { get; set; }
}
