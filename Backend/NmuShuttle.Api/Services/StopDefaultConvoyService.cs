using Microsoft.EntityFrameworkCore;
using NmuShuttle.Api.Data;
using NmuShuttle.Api.Models;

namespace NmuShuttle.Api.Services;

/// <summary>
/// Manages StopShuttleDefault records with accessible-first ordering.
/// No date enforcement — edits only affect future injection runs.
/// </summary>
public class StopDefaultConvoyService : IStopDefaultConvoyService
{
    private readonly ShuttleDbContext _db;

    public StopDefaultConvoyService(ShuttleDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public async Task<StopShuttleDefault> AssignShuttleToDefaultAsync(int stopId, int shuttleId)
    {
        var shuttle = await _db.Shuttles.FindAsync(shuttleId)
            ?? throw new KeyNotFoundException($"Shuttle with ID {shuttleId} not found.");

        if (!shuttle.IsActive)
            throw new InvalidOperationException(
                $"Shuttle {shuttle.LicensePlate} is inactive and cannot be added to a convoy.");

        // Check for duplicate on this stop
        var exists = await _db.StopShuttleDefaults
            .AnyAsync(d => d.StopId == stopId && d.ShuttleId == shuttleId);
        if (exists)
            throw new InvalidOperationException(
                $"Shuttle {shuttle.LicensePlate} is already in the default convoy for this stop.");

        // Check if shuttle is assigned to any other stop's convoy
        var otherStop = await _db.StopShuttleDefaults
            .Include(d => d.Stop)
            .FirstOrDefaultAsync(d => d.ShuttleId == shuttleId && d.StopId != stopId);
        if (otherStop != null)
            throw new InvalidOperationException(
                $"Shuttle {shuttle.LicensePlate} is already assigned to {otherStop.Stop.StopName}.");

        var existingDefaults = await _db.StopShuttleDefaults
            .Where(d => d.StopId == stopId)
            .OrderBy(d => d.OrderPosition)
            .ToListAsync();

        int newPosition;

        if (shuttle.IsAccessibleVehicle)
        {
            // Insert after last accessible shuttle
            var lastAccessiblePosition = 0;
            foreach (var def in existingDefaults)
            {
                var defShuttle = await _db.Shuttles.FindAsync(def.ShuttleId);
                if (defShuttle != null && defShuttle.IsAccessibleVehicle)
                    lastAccessiblePosition = def.OrderPosition;
            }

            newPosition = lastAccessiblePosition + 1;

            // Shift non-accessible entries up by 1
            var toShift = existingDefaults
                .Where(d => d.OrderPosition >= newPosition)
                .OrderByDescending(d => d.OrderPosition)
                .ToList();

            foreach (var def in toShift)
                def.OrderPosition += 1;
        }
        else
        {
            // Append at end
            newPosition = existingDefaults.Count > 0
                ? existingDefaults.Max(d => d.OrderPosition) + 1
                : 1;
        }

        var newDefault = new StopShuttleDefault
        {
            StopId = stopId,
            ShuttleId = shuttleId,
            OrderPosition = newPosition,
            CreatedAt = DateTime.UtcNow
        };

        _db.StopShuttleDefaults.Add(newDefault);
        await _db.SaveChangesAsync();

        return newDefault;
    }

    /// <inheritdoc />
    public async Task RemoveShuttleFromDefaultAsync(int stopId, int shuttleId)
    {
        var record = await _db.StopShuttleDefaults
            .FirstOrDefaultAsync(d => d.StopId == stopId && d.ShuttleId == shuttleId)
            ?? throw new KeyNotFoundException(
                $"Shuttle {shuttleId} is not in the default convoy for stop {stopId}.");

        _db.StopShuttleDefaults.Remove(record);
        await _db.SaveChangesAsync();

        // Re-normalise positions
        await RenormalisePositionsAsync(stopId);
    }

    /// <inheritdoc />
    public async Task ReorderDefaultAsync(int stopId, int[] orderedShuttleIds)
    {
        var defaults = await _db.StopShuttleDefaults
            .Include(d => d.Shuttle)
            .Where(d => d.StopId == stopId)
            .ToListAsync();

        if (defaults.Count != orderedShuttleIds.Length)
            throw new InvalidOperationException(
                "Provided shuttle list does not match the number of shuttles in the default convoy.");

        // Validate accessible-first invariant
        var seenNonAccessible = false;
        foreach (var shuttleId in orderedShuttleIds)
        {
            var def = defaults.FirstOrDefault(d => d.ShuttleId == shuttleId)
                ?? throw new InvalidOperationException(
                    $"Shuttle {shuttleId} is not in the default convoy for this stop.");

            if (def.Shuttle.IsAccessibleVehicle && seenNonAccessible)
                throw new InvalidOperationException(
                    "Accessible shuttles must be positioned before non-accessible shuttles.");

            if (!def.Shuttle.IsAccessibleVehicle)
                seenNonAccessible = true;
        }

        // Apply new order
        for (int i = 0; i < orderedShuttleIds.Length; i++)
        {
            var def = defaults.First(d => d.ShuttleId == orderedShuttleIds[i]);
            def.OrderPosition = i + 1;
        }

        await _db.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<IList<StopShuttleDefault>> GetDefaultConvoyAsync(int stopId)
    {
        return await _db.StopShuttleDefaults
            .Include(d => d.Shuttle)
            .Where(d => d.StopId == stopId)
            .OrderBy(d => d.OrderPosition)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task CleanupDecommissionedShuttleAsync(int shuttleId)
    {
        var affectedDefaults = await _db.StopShuttleDefaults
            .Where(d => d.ShuttleId == shuttleId)
            .ToListAsync();

        if (affectedDefaults.Count == 0)
            return;

        var affectedStopIds = affectedDefaults.Select(d => d.StopId).Distinct().ToList();

        _db.StopShuttleDefaults.RemoveRange(affectedDefaults);
        await _db.SaveChangesAsync();

        // Re-normalise positions for each affected stop
        foreach (var stopId in affectedStopIds)
        {
            await RenormalisePositionsAsync(stopId);
        }
    }

    /// <summary>
    /// Re-assigns contiguous 1-based OrderPositions preserving current relative order.
    /// </summary>
    private async Task RenormalisePositionsAsync(int stopId)
    {
        var remaining = await _db.StopShuttleDefaults
            .Where(d => d.StopId == stopId)
            .OrderBy(d => d.OrderPosition)
            .ToListAsync();

        for (int i = 0; i < remaining.Count; i++)
        {
            remaining[i].OrderPosition = i + 1;
        }

        await _db.SaveChangesAsync();
    }
}
