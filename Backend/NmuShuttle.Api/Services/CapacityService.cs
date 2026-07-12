using Microsoft.EntityFrameworkCore;
using NmuShuttle.Api.Data;

namespace NmuShuttle.Api.Services;

/// <summary>
/// Calculates seat capacity for a trip instance based on operational shuttle allocations.
/// </summary>
public class CapacityService : ICapacityService
{
    private readonly ShuttleDbContext _db;

    public CapacityService(ShuttleDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public async Task<int> GetTotalCapacityAsync(int tripInstanceId)
    {
        return await _db.TripShuttleAllocations
            .Where(a => a.TripInstanceId == tripInstanceId && a.IsOperational)
            .Join(
                _db.Shuttles,
                allocation => allocation.ShuttleId,
                shuttle => shuttle.ShuttleId,
                (allocation, shuttle) => shuttle.Capacity)
            .SumAsync(capacity => capacity);
    }

    /// <inheritdoc />
    public async Task<int> GetConfirmedCountAsync(int tripInstanceId)
    {
        return await _db.Bookings
            .CountAsync(b => b.TripInstanceId == tripInstanceId && b.BookingStatus == "Confirmed");
    }

    /// <inheritdoc />
    public async Task<int> GetAvailableSeatsAsync(int tripInstanceId)
    {
        var total = await GetTotalCapacityAsync(tripInstanceId);
        var confirmed = await GetConfirmedCountAsync(tripInstanceId);
        return Math.Max(0, total - confirmed);
    }
}
