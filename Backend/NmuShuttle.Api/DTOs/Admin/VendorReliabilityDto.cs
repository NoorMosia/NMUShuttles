namespace NmuShuttle.Api.DTOs.Admin;

public class VendorReliabilityDto
{
    public List<ShuttleReliabilityDto> PerShuttle { get; set; } = new();
    public List<VendorBreakdownSummaryDto> PerVendor { get; set; } = new();
}

public class ShuttleReliabilityDto
{
    public int ShuttleId { get; set; }
    public string LicensePlate { get; set; } = string.Empty;
    public int BreakdownCount { get; set; }
    public int CancellationCount { get; set; }
}

public class VendorBreakdownSummaryDto
{
    public string VendorCompanyName { get; set; } = string.Empty;
    public int TotalBreakdowns { get; set; }
    public int TotalCancellations { get; set; }
}
