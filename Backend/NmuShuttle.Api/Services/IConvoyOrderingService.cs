using NmuShuttle.Api.Models;

namespace NmuShuttle.Api.Services;

/// <summary>
/// Manages convoy ordering — accessible shuttles at front (lowest OrderPosition),
/// non-accessible shuttles appended after.
/// </summary>
public interface IConvoyOrderingService
{
    /// <summary>
    /// Assigns a shuttle to a trip instance with correct convoy ordering.
    /// Accessible shuttles receive the lowest available positions; non-accessible are appended.
    /// </summary>
    Task<TripShuttleAllocation> AssignShuttleToTripAsync(int tripInstanceId, int shuttleId);

    /// <summary>
    /// Reorders all allocations for a trip so that accessible shuttles come first,
    /// then non-accessible, with contiguous 1-based OrderPosition values.
    /// </summary>
    Task ReorderConvoyAsync(int tripInstanceId);
}
