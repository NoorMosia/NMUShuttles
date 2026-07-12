using NmuShuttle.Api.Models;

namespace NmuShuttle.Api.Services;

/// <summary>
/// Handles shuttle breakdown flagging by Marshals and resolution by Admins.
/// </summary>
public interface IBreakdownService
{
    /// <summary>
    /// Marshal flags a shuttle breakdown. Sets allocation non-operational,
    /// creates a BreakdownLog, and transitions trip to IncidentActive.
    /// No notifications are dispatched at flag time.
    /// </summary>
    Task<BreakdownLog> FlagBreakdownAsync(int tripInstanceId, int shuttleId, int marshalId);

    /// <summary>
    /// Admin resolves a breakdown by replacing the shuttle or cancelling.
    /// Dispatches FCM notifications to all confirmed booking holders on the trip.
    /// </summary>
    Task<BreakdownLog> ResolveBreakdownAsync(int breakdownLogId, string resolutionType, int? replacementShuttleId, int adminId);
}
