using Microsoft.EntityFrameworkCore;
using NmuShuttle.Api.Data;
using NmuShuttle.Api.Models;

namespace NmuShuttle.Api.Services;

/// <summary>
/// Implements the trip status state machine with defined valid transitions.
/// Records timestamps and applies side effects on status changes.
/// </summary>
public class TripStateMachineService : ITripStateMachineService
{
    private readonly ShuttleDbContext _context;

    /// <summary>
    /// Set of valid (from, to) status transitions.
    /// </summary>
    private static readonly HashSet<(string From, string To)> ValidTransitions = new()
    {
        ("Scheduling", "Locked"),
        ("Scheduling", "IncidentActive"),
        ("Locked", "InTransit"),
        ("InTransit", "Completed"),
        ("InTransit", "IncidentActive"),
        ("IncidentActive", "InTransit")
    };

    public TripStateMachineService(ShuttleDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public bool IsValidTransition(string fromStatus, string toStatus)
    {
        return ValidTransitions.Contains((fromStatus, toStatus));
    }

    /// <inheritdoc />
    public async Task<TripInstance> TransitionAsync(int tripInstanceId, string toStatus)
    {
        var tripInstance = await _context.TripInstances
            .Include(t => t.Bookings)
            .FirstOrDefaultAsync(t => t.TripInstanceId == tripInstanceId);

        if (tripInstance == null)
        {
            throw new KeyNotFoundException($"TripInstance with ID {tripInstanceId} was not found.");
        }

        if (!IsValidTransition(tripInstance.Status, toStatus))
        {
            throw new InvalidOperationException(
                $"Invalid status transition from '{tripInstance.Status}' to '{toStatus}'.");
        }

        // Record timestamps based on target status
        var now = DateTime.UtcNow;
        switch (toStatus)
        {
            case "Locked":
                tripInstance.LockedAt = now;
                break;
            case "InTransit":
                tripInstance.InTransitAt = now;
                break;
            case "Completed":
                tripInstance.CompletedAt = now;
                // Mark all confirmed bookings with Pending boarded status as NoShow
                foreach (var booking in tripInstance.Bookings)
                {
                    if (booking.BookingStatus == "Confirmed" && booking.BoardedStatus == "Pending")
                    {
                        booking.BoardedStatus = "NoShow";
                    }
                }
                break;
        }

        tripInstance.Status = toStatus;

        await _context.SaveChangesAsync();

        return tripInstance;
    }
}
