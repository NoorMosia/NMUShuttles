using Microsoft.EntityFrameworkCore;
using NmuShuttle.Api.Data;
using NmuShuttle.Api.Models;

namespace NmuShuttle.Api.Services;

/// <summary>
/// Manages ScheduleTimeCampus records — ordered campus waypoints per route.
/// </summary>
public class ScheduleTimeCampusService : IScheduleTimeCampusService
{
    private readonly ShuttleDbContext _db;

    public ScheduleTimeCampusService(ShuttleDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public async Task<ScheduleTimeCampus> AddCampusToRouteAsync(int scheduleTimeId, int campusId, int? stopOrder = null)
    {
        // Validate campus exists
        var campus = await _db.Campuses.FindAsync(campusId)
            ?? throw new KeyNotFoundException($"Campus {campusId} not found.");

        // Validate schedule time exists
        _ = await _db.ScheduleTimes.FindAsync(scheduleTimeId)
            ?? throw new KeyNotFoundException($"ScheduleTime {scheduleTimeId} not found.");

        // Check for duplicate
        var exists = await _db.ScheduleTimeCampuses
            .AnyAsync(stc => stc.ScheduleTimeId == scheduleTimeId && stc.CampusId == campusId);
        if (exists)
            throw new InvalidOperationException(
                $"Campus {campus.CampusName} is already on this route.");

        var existing = await _db.ScheduleTimeCampuses
            .Where(stc => stc.ScheduleTimeId == scheduleTimeId)
            .OrderBy(stc => stc.StopOrder)
            .ToListAsync();

        int newOrder = stopOrder ?? (existing.Count > 0 ? existing.Max(e => e.StopOrder) + 1 : 1);

        // If inserting at a position, shift existing entries
        if (stopOrder.HasValue)
        {
            var toShift = existing
                .Where(e => e.StopOrder >= newOrder)
                .OrderByDescending(e => e.StopOrder)
                .ToList();

            foreach (var entry in toShift)
                entry.StopOrder += 1;
        }

        var record = new ScheduleTimeCampus
        {
            ScheduleTimeId = scheduleTimeId,
            CampusId = campusId,
            StopOrder = newOrder
        };

        _db.ScheduleTimeCampuses.Add(record);
        await _db.SaveChangesAsync();

        return record;
    }

    /// <inheritdoc />
    public async Task RemoveCampusFromRouteAsync(int scheduleTimeId, int campusId)
    {
        var record = await _db.ScheduleTimeCampuses
            .FirstOrDefaultAsync(stc => stc.ScheduleTimeId == scheduleTimeId && stc.CampusId == campusId)
            ?? throw new KeyNotFoundException(
                $"Campus {campusId} is not on the route for ScheduleTime {scheduleTimeId}.");

        _db.ScheduleTimeCampuses.Remove(record);
        await _db.SaveChangesAsync();

        // Re-normalise
        await RenormaliseStopOrderAsync(scheduleTimeId);
    }

    /// <inheritdoc />
    public async Task ReorderRouteAsync(int scheduleTimeId, int[] orderedCampusIds)
    {
        var entries = await _db.ScheduleTimeCampuses
            .Where(stc => stc.ScheduleTimeId == scheduleTimeId)
            .ToListAsync();

        if (entries.Count != orderedCampusIds.Length)
            throw new InvalidOperationException(
                "Provided campus list does not match the number of campuses on this route.");

        for (int i = 0; i < orderedCampusIds.Length; i++)
        {
            var entry = entries.FirstOrDefault(e => e.CampusId == orderedCampusIds[i])
                ?? throw new InvalidOperationException(
                    $"Campus {orderedCampusIds[i]} is not on this route.");

            entry.StopOrder = i + 1;
        }

        await _db.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<IList<ScheduleTimeCampus>> GetRouteCampusesAsync(int scheduleTimeId)
    {
        return await _db.ScheduleTimeCampuses
            .Include(stc => stc.Campus)
            .Where(stc => stc.ScheduleTimeId == scheduleTimeId)
            .OrderBy(stc => stc.StopOrder)
            .ToListAsync();
    }

    private async Task RenormaliseStopOrderAsync(int scheduleTimeId)
    {
        var remaining = await _db.ScheduleTimeCampuses
            .Where(stc => stc.ScheduleTimeId == scheduleTimeId)
            .OrderBy(stc => stc.StopOrder)
            .ToListAsync();

        for (int i = 0; i < remaining.Count; i++)
        {
            remaining[i].StopOrder = i + 1;
        }

        await _db.SaveChangesAsync();
    }
}
