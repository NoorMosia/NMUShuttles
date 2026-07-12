namespace NmuShuttle.Api.DTOs.Admin;

/// <summary>
/// Full report details for admin view.
/// </summary>
public class AdminReportResponseDto
{
    public int ReportId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int StudentId { get; set; }
    public string StudentNumber { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public int? TripInstanceId { get; set; }
    public string? AdminNotes { get; set; }
    public DateTime SubmissionDate { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
