namespace NmuShuttle.Api.DTOs.Admin;

public class DetailedAnalyticsDto
{
    public List<StopBookingDemandDto> PerStopDemand { get; set; } = new();
    public OverallBookingStatsDto OverallStats { get; set; } = new();
}

public class StopBookingDemandDto
{
    public int StopId { get; set; }
    public string StopName { get; set; } = string.Empty;
    public int TotalBookings { get; set; }
    public int ConfirmedBookings { get; set; }
    public int WaitlistedBookings { get; set; }
}

public class OverallBookingStatsDto
{
    public int Total { get; set; }
    public int Confirmed { get; set; }
    public int Waitlisted { get; set; }
    public int Cancelled { get; set; }
}
