using Microsoft.EntityFrameworkCore;
using NmuShuttle.Api.Data;
using NmuShuttle.Api.DTOs.Admin;

namespace NmuShuttle.Api.Services;

public class VendorAnalyticsService : IVendorAnalyticsService
{
    private readonly ShuttleDbContext _context;

    public VendorAnalyticsService(ShuttleDbContext context)
    {
        _context = context;
    }

    public async Task<OverviewDto> GetOverviewAsync()
    {
        // Top 3 most in-demand stops (highest confirmed booking counts)
        var stopDemand = await _context.Bookings
            .Where(b => b.BookingStatus == "Confirmed")
            .Join(_context.TripInstances,
                b => b.TripInstanceId,
                t => t.TripInstanceId,
                (b, t) => new { t.StopId })
            .GroupBy(x => x.StopId)
            .Select(g => new { StopId = g.Key, Count = g.Count() })
            .ToListAsync();

        var stopIds = stopDemand.Select(s => s.StopId).ToList();
        var stops = await _context.Stops
            .Where(s => stopIds.Contains(s.StopId))
            .ToDictionaryAsync(s => s.StopId, s => s.StopName);

        var demandList = stopDemand
            .Select(s => new StopDemandDto
            {
                StopId = s.StopId,
                StopName = stops.GetValueOrDefault(s.StopId, "Unknown"),
                ConfirmedBookingCount = s.Count
            })
            .OrderByDescending(s => s.ConfirmedBookingCount)
            .ToList();

        var mostInDemand = demandList.Take(3).ToList();
        var leastInDemand = demandList.OrderBy(s => s.ConfirmedBookingCount).Take(3).ToList();

        // Report status summary
        var reportCounts = await _context.Reports
            .GroupBy(r => r.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var reportSummary = new ReportStatusSummaryDto
        {
            Submitted = reportCounts.FirstOrDefault(r => r.Status == "Submitted")?.Count ?? 0,
            UnderReview = reportCounts.FirstOrDefault(r => r.Status == "UnderReview")?.Count ?? 0,
            Resolved = reportCounts.FirstOrDefault(r => r.Status == "Resolved")?.Count ?? 0
        };

        // Breakdown/cancellation tallies
        var totalBreakdowns = await _context.BreakdownLogs.CountAsync();
        var totalCancellations = await _context.BreakdownLogs
            .CountAsync(bl => bl.ResolutionType == "Cancelled");

        return new OverviewDto
        {
            MostInDemandStops = mostInDemand,
            LeastInDemandStops = leastInDemand,
            ReportStatusSummary = reportSummary,
            BreakdownCancellationTally = new BreakdownCancellationTallyDto
            {
                TotalBreakdowns = totalBreakdowns,
                TotalCancellations = totalCancellations
            }
        };
    }

    public async Task<DetailedAnalyticsDto> GetDetailedAsync()
    {
        // Per-stop booking demand
        var perStop = await _context.Bookings
            .Join(_context.TripInstances,
                b => b.TripInstanceId,
                t => t.TripInstanceId,
                (b, t) => new { b.BookingStatus, t.StopId })
            .GroupBy(x => x.StopId)
            .Select(g => new
            {
                StopId = g.Key,
                TotalBookings = g.Count(),
                ConfirmedBookings = g.Count(x => x.BookingStatus == "Confirmed"),
                WaitlistedBookings = g.Count(x => x.BookingStatus == "Waitlisted")
            })
            .ToListAsync();

        var stopIds = perStop.Select(s => s.StopId).ToList();
        var stops = await _context.Stops
            .Where(s => stopIds.Contains(s.StopId))
            .ToDictionaryAsync(s => s.StopId, s => s.StopName);

        var perStopDemand = perStop
            .Select(s => new StopBookingDemandDto
            {
                StopId = s.StopId,
                StopName = stops.GetValueOrDefault(s.StopId, "Unknown"),
                TotalBookings = s.TotalBookings,
                ConfirmedBookings = s.ConfirmedBookings,
                WaitlistedBookings = s.WaitlistedBookings
            })
            .OrderByDescending(s => s.TotalBookings)
            .ToList();

        // Overall booking stats
        var overallStats = await _context.Bookings
            .GroupBy(_ => 1)
            .Select(g => new OverallBookingStatsDto
            {
                Total = g.Count(),
                Confirmed = g.Count(b => b.BookingStatus == "Confirmed"),
                Waitlisted = g.Count(b => b.BookingStatus == "Waitlisted"),
                Cancelled = g.Count(b => b.BookingStatus == "Cancelled")
            })
            .FirstOrDefaultAsync() ?? new OverallBookingStatsDto();

        return new DetailedAnalyticsDto
        {
            PerStopDemand = perStopDemand,
            OverallStats = overallStats
        };
    }

    public async Task<VendorReliabilityDto> GetVendorReliabilityAsync(DateOnly? fromDate, DateOnly? toDate)
    {
        var query = _context.BreakdownLogs.AsQueryable();

        // Filter by FlaggedAt date range if provided
        if (fromDate.HasValue)
        {
            var from = fromDate.Value.ToDateTime(TimeOnly.MinValue);
            query = query.Where(bl => bl.FlaggedAt >= from);
        }

        if (toDate.HasValue)
        {
            var to = toDate.Value.ToDateTime(TimeOnly.MaxValue);
            query = query.Where(bl => bl.FlaggedAt <= to);
        }

        // Per-shuttle breakdown/cancellation counts
        var perShuttleData = await query
            .GroupBy(bl => bl.ShuttleId)
            .Select(g => new
            {
                ShuttleId = g.Key,
                BreakdownCount = g.Count(),
                CancellationCount = g.Count(bl => bl.ResolutionType == "Cancelled")
            })
            .ToListAsync();

        var shuttleIds = perShuttleData.Select(s => s.ShuttleId).ToList();
        var shuttles = await _context.Shuttles
            .Where(s => shuttleIds.Contains(s.ShuttleId))
            .ToDictionaryAsync(s => s.ShuttleId, s => s.LicensePlate);

        var perShuttle = perShuttleData
            .Select(s => new ShuttleReliabilityDto
            {
                ShuttleId = s.ShuttleId,
                LicensePlate = shuttles.GetValueOrDefault(s.ShuttleId, "Unknown"),
                BreakdownCount = s.BreakdownCount,
                CancellationCount = s.CancellationCount
            })
            .OrderByDescending(s => s.BreakdownCount)
            .ToList();

        // Per-vendor breakdown/cancellation counts
        var perVendor = await query
            .GroupBy(bl => bl.VendorCompanyName)
            .Select(g => new VendorBreakdownSummaryDto
            {
                VendorCompanyName = g.Key,
                TotalBreakdowns = g.Count(),
                TotalCancellations = g.Count(bl => bl.ResolutionType == "Cancelled")
            })
            .OrderByDescending(v => v.TotalBreakdowns)
            .ToListAsync();

        return new VendorReliabilityDto
        {
            PerShuttle = perShuttle,
            PerVendor = perVendor
        };
    }
}
