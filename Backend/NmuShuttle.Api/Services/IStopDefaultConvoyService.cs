using NmuShuttle.Api.Models;

namespace NmuShuttle.Api.Services;

/// <summary>
/// Manages StopShuttleDefault records — the standing default convoy roster per Stop.
/// Accessible-first ordering is enforced. No date enforcement needed.
/// </summary>
public interface IStopDefaultConvoyService
{
    /// <summary>
    /// Adds an active shuttle to a Stop's default convoy with accessible-first ordering.
    /// </summary>
    Task<StopShuttleDefault> AssignShuttleToDefaultAsync(int stopId, int shuttleId);

    /// <summary>
    /// Removes a shuttle from a Stop's default convoy and re-normalises positions.
    /// </summary>
    Task RemoveShuttleFromDefaultAsync(int stopId, int shuttleId);

    /// <summary>
    /// Reorders the default convoy for a Stop. Validates accessible-first invariant.
    /// </summary>
    Task ReorderDefaultAsync(int stopId, int[] orderedShuttleIds);

    /// <summary>
    /// Returns the default convoy for a Stop, ordered by OrderPosition.
    /// </summary>
    Task<IList<StopShuttleDefault>> GetDefaultConvoyAsync(int stopId);

    /// <summary>
    /// Removes a deactivated shuttle from all Stop defaults and re-normalises positions.
    /// Called when a shuttle's IsActive is set to false.
    /// </summary>
    Task CleanupDecommissionedShuttleAsync(int shuttleId);
}
