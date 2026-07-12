using Microsoft.EntityFrameworkCore;
using NmuShuttle.Api.Data;
using NmuShuttle.Api.Models;

namespace NmuShuttle.Api.Services;

/// <summary>
/// Manages convoy ordering — accessible shuttles at front (lowest OrderPosition),
/// non-accessible shuttles appended after.
/// </summary>
public class ConvoyOrderingService : IConvoyOrderingService
{
    private readonly ShuttleDbContext _db;

    public ConvoyOrderingService(ShuttleDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public async Task<TripShuttleAllocation> AssignShuttleToTripAsync(int tripInstanceId, int shuttleId)
    {
        var shuttle = await _db.Shuttles.FindAsync(shuttleId)
            ?? throw new InvalidOperationException($"Shuttle with ID {shuttleId} not found.");

        var existingAllocations = await _db.TripShuttleAllocations
            .Where(a => a.TripInstanceId == tripInstanceId)
            .OrderBy(a => a.OrderPosition)
            .ToListAsync();

        int newPosition;

        if (shuttle.IsAccessibleVehicle)
        {
            // Find the position after the last accessible shuttle (or position 1 if none exist)
            var lastAccessiblePosition = 0;

            foreach (var allocation in existingAllocations)
            {
                var allocShuttle = await _db.Shuttles.FindAsync(allocation.ShuttleId);
                if (allocShuttle != null && allocShuttle.IsAccessibleVehicle)
                {
                    lastAccessiblePosition = allocation.OrderPosition;
                }
            }

            newPosition = lastAccessiblePosition + 1;

            // Shift all non-accessible allocations up by 1
            var allocationsToShift = existingAllocations
                .Where(a => a.OrderPosition >= newPosition)
                .OrderByDescending(a => a.OrderPosition)
                .ToList();

            foreach (var allocation in allocationsToShift)
            {
                allocation.OrderPosition += 1;
            }
        }
        else
        {
            // Non-accessible: append at the end
            newPosition = existingAllocations.Count > 0
                ? existingAllocations.Max(a => a.OrderPosition) + 1
                : 1;
        }

        var newAllocation = new TripShuttleAllocation
        {
            TripInstanceId = tripInstanceId,
            ShuttleId = shuttleId,
            OrderPosition = newPosition,
            IsOperational = true,
            AssignedAt = DateTime.UtcNow
        };

        _db.TripShuttleAllocations.Add(newAllocation);
        await _db.SaveChangesAsync();

        return newAllocation;
    }

    /// <inheritdoc />
    public async Task ReorderConvoyAsync(int tripInstanceId)
    {
        var allocations = await _db.TripShuttleAllocations
            .Include(a => a.Shuttle)
            .Where(a => a.TripInstanceId == tripInstanceId)
            .ToListAsync();

        // Sort: accessible first (by current OrderPosition), then non-accessible (by current OrderPosition)
        var sorted = allocations
            .OrderByDescending(a => a.Shuttle.IsAccessibleVehicle)
            .ThenBy(a => a.OrderPosition)
            .ToList();

        // Reassign contiguous 1-based positions
        for (int i = 0; i < sorted.Count; i++)
        {
            sorted[i].OrderPosition = i + 1;
        }

        await _db.SaveChangesAsync();
    }
}
