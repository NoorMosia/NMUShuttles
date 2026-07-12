using NmuShuttle.Api.Models;

namespace NmuShuttle.Api.Services;

/// <summary>
/// Manages trip status transitions according to the defined state machine rules.
/// </summary>
public interface ITripStateMachineService
{
    /// <summary>
    /// Returns true if the transition from one status to another is valid.
    /// </summary>
    bool IsValidTransition(string fromStatus, string toStatus);

    /// <summary>
    /// Transitions a trip instance to the specified status, recording timestamps
    /// and applying side effects (e.g., marking no-shows on completion).
    /// </summary>
    Task<TripInstance> TransitionAsync(int tripInstanceId, string toStatus);
}
