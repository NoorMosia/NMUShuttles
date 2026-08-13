namespace NmuShuttle.Api.DTOs.Admin;

/// <summary>
/// Full report details for admin view, including rich trip information.
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
    public int? BookingId { get; set; }
    public string? AdminNotes { get; set; }
    public DateTime SubmissionDate { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Rich trip details (populated when TripInstanceId is present)
    public TripDetailsDto? TripDetails { get; set; }
}

/// <summary>
/// Trip information shown on the admin report detail view.
/// </summary>
public class TripDetailsDto
{
    public int TripInstanceId { get; set; }
    public string TripDate { get; set; } = string.Empty;
    public string DepartureTime { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public string StopName { get; set; } = string.Empty;
    public string TripStatus { get; set; } = string.Empty;
    public string? ShuttlePlate { get; set; }
    public string? VendorName { get; set; }
}
