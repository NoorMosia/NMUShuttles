namespace NmuShuttle.Api.DTOs.Admin;

public class OverviewDto
{
    public List<StopDemandDto> MostInDemandStops { get; set; } = new();
    public List<StopDemandDto> LeastInDemandStops { get; set; } = new();
    public ReportStatusSummaryDto ReportStatusSummary { get; set; } = new();
    public BreakdownCancellationTallyDto BreakdownCancellationTally { get; set; } = new();
}

public class StopDemandDto
{
    public int StopId { get; set; }
    public string StopName { get; set; } = string.Empty;
    public int ConfirmedBookingCount { get; set; }
}

public class ReportStatusSummaryDto
{
    public int Submitted { get; set; }
    public int UnderReview { get; set; }
    public int Resolved { get; set; }
}

public class BreakdownCancellationTallyDto
{
    public int TotalBreakdowns { get; set; }
    public int TotalCancellations { get; set; }
}
