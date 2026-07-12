using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NmuShuttle.Api.Services;

namespace NmuShuttle.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/analytics")]
[Authorize(Roles = "Admin")]
public class AnalyticsAdminController : ControllerBase
{
    private readonly IVendorAnalyticsService _analyticsService;

    public AnalyticsAdminController(IVendorAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    /// <summary>
    /// Returns an overview of analytics: top/bottom 3 stops by demand,
    /// report status counts, and breakdown/cancellation tallies.
    /// </summary>
    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview()
    {
        var result = await _analyticsService.GetOverviewAsync();
        return Ok(result);
    }

    /// <summary>
    /// Returns detailed booking demand analytics per stop and overall stats.
    /// </summary>
    [HttpGet("detailed")]
    public async Task<IActionResult> GetDetailed()
    {
        var result = await _analyticsService.GetDetailedAsync();
        return Ok(result);
    }

    /// <summary>
    /// Returns vendor reliability data: per-shuttle and per-vendor
    /// breakdown/cancellation counts, optionally filtered by date range.
    /// </summary>
    [HttpGet("vendor-reliability")]
    public async Task<IActionResult> GetVendorReliability(
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate)
    {
        var result = await _analyticsService.GetVendorReliabilityAsync(fromDate, toDate);
        return Ok(result);
    }
}
