using NmuShuttle.Api.Models;

namespace NmuShuttle.Api.Services;

/// <summary>
/// Manages ScheduleTimeCampus records — the ordered sequence of campuses on a route.
/// Enforces unique StopOrder and contiguous 1-based ordering.
/// </summary>
public interface IScheduleTimeCampusService
{
    /// <summary>
    /// Adds a campus to a route at the specified stop order (or appends at end).
    /// </summary>
    Task<ScheduleTimeCampus> AddCampusToRouteAsync(int scheduleTimeId, int campusId, int? stopOrder = null);

    /// <summary>
    /// Removes a campus from a route and re-normalises StopOrder values.
    /// </summary>
    Task RemoveCampusFromRouteAsync(int scheduleTimeId, int campusId);

    /// <summary>
    /// Reorders campuses on a route. Validates contiguous ordering.
    /// </summary>
    Task ReorderRouteAsync(int scheduleTimeId, int[] orderedCampusIds);

    /// <summary>
    /// Returns the ordered list of campuses for a ScheduleTime.
    /// </summary>
    Task<IList<ScheduleTimeCampus>> GetRouteCampusesAsync(int scheduleTimeId);
}
