using Microsoft.EntityFrameworkCore;
using NmuShuttle.Api.Data;
using NmuShuttle.Api.Models;

namespace NmuShuttle.Api.Services;

/// <summary>
/// Implements shuttle breakdown flagging and admin resolution workflow.
/// </summary>
public class BreakdownService : IBreakdownService
{
    private readonly ShuttleDbContext _db;
    private readonly INotificationService _notificationService;

    public BreakdownService(ShuttleDbContext db, INotificationService notificationService)
    {
        _db = db;
        _notificationService = notificationService;
    }

    /// <inheritdoc />
    public async Task<BreakdownLog> FlagBreakdownAsync(int tripInstanceId, int shuttleId, int marshalId)
    {
        // Find the allocation for this trip + shuttle
        var allocation = await _db.TripShuttleAllocations
            .FirstOrDefaultAsync(a => a.TripInstanceId == tripInstanceId && a.ShuttleId == shuttleId)
            ?? throw new KeyNotFoundException(
                $"No allocation found for TripInstance {tripInstanceId} and Shuttle {shuttleId}.");

        // Mark shuttle as non-operational
        allocation.IsOperational = false;

        // Get shuttle to denormalize VendorCompanyName
        var shuttle = await _db.Shuttles.FindAsync(shuttleId)
            ?? throw new KeyNotFoundException($"Shuttle {shuttleId} not found.");

        // Create the breakdown log
        var breakdownLog = new BreakdownLog
        {
            TripInstanceId = tripInstanceId,
            ShuttleId = shuttleId,
            VendorCompanyName = shuttle.VendorCompanyName,
            FlaggedAt = DateTime.UtcNow,
            FlaggedByMarshalId = marshalId
        };
        _db.BreakdownLogs.Add(breakdownLog);

        // Transition trip status to IncidentActive
        var tripInstance = await _db.TripInstances.FindAsync(tripInstanceId)
            ?? throw new KeyNotFoundException($"TripInstance {tripInstanceId} not found.");

        tripInstance.Status = "IncidentActive";

        await _db.SaveChangesAsync();

        // NO notifications dispatched at flag time
        return breakdownLog;
    }

    /// <inheritdoc />
    public async Task<BreakdownLog> ResolveBreakdownAsync(
        int breakdownLogId,
        string resolutionType,
        int? replacementShuttleId,
        int adminId)
    {
        var breakdownLog = await _db.BreakdownLogs.FindAsync(breakdownLogId)
            ?? throw new KeyNotFoundException($"BreakdownLog {breakdownLogId} not found.");

        if (resolutionType == "Replaced")
        {
            if (replacementShuttleId is null)
                throw new ArgumentException(
                    "ReplacementShuttleId is required when resolution type is 'Replaced'.");

            // Update the allocation: swap shuttle and restore operational status
            var allocation = await _db.TripShuttleAllocations
                .FirstOrDefaultAsync(a =>
                    a.TripInstanceId == breakdownLog.TripInstanceId &&
                    a.ShuttleId == breakdownLog.ShuttleId)
                ?? throw new KeyNotFoundException(
                    $"No allocation found for TripInstance {breakdownLog.TripInstanceId} and Shuttle {breakdownLog.ShuttleId}.");

            allocation.ShuttleId = replacementShuttleId.Value;
            allocation.IsOperational = true;

            // Restore trip status to InTransit
            var tripInstance = await _db.TripInstances.FindAsync(breakdownLog.TripInstanceId)
                ?? throw new KeyNotFoundException(
                    $"TripInstance {breakdownLog.TripInstanceId} not found.");

            tripInstance.Status = "InTransit";

            breakdownLog.ReplacementShuttleId = replacementShuttleId.Value;
        }
        else if (resolutionType == "Cancelled")
        {
            // IsOperational remains false — no allocation changes needed
            // Trip status remains as-is (IncidentActive or as admin decides)
        }

        // Mark resolution on the breakdown log
        breakdownLog.ResolutionType = resolutionType;
        breakdownLog.ResolvedAt = DateTime.UtcNow;
        breakdownLog.ResolvedByAdminId = adminId;

        await _db.SaveChangesAsync();

        // Dispatch notifications to all confirmed booking holders on the trip
        var notificationMessage = resolutionType == "Replaced"
            ? "A replacement shuttle has been assigned to your trip."
            : "A shuttle on your trip has been cancelled. Service may be affected.";

        await _notificationService.DispatchToTripPassengersAsync(
            breakdownLog.TripInstanceId,
            "Shuttle Breakdown Update",
            notificationMessage);

        return breakdownLog;
    }
}
