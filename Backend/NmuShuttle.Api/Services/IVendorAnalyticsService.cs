using NmuShuttle.Api.DTOs.Admin;

namespace NmuShuttle.Api.Services;

public interface IVendorAnalyticsService
{
    Task<OverviewDto> GetOverviewAsync();
    Task<DetailedAnalyticsDto> GetDetailedAsync();
    Task<VendorReliabilityDto> GetVendorReliabilityAsync(DateOnly? fromDate, DateOnly? toDate);
}
