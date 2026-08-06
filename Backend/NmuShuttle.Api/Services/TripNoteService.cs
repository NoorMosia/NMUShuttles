using Microsoft.EntityFrameworkCore;
using NmuShuttle.Api.Data;
using NmuShuttle.Api.Models;

namespace NmuShuttle.Api.Services;

/// <summary>
/// Manages TripNote creation with incident resolution trigger logic.
/// </summary>
public class TripNoteService : ITripNoteService
{
    private readonly ShuttleDbContext _db;
    private readonly INotificationService _notificationService;

    public TripNoteService(ShuttleDbContext db, INotificationService notificationService)
    {
        _db = db;
        _notificationService = notificationService;
    }

    /// <inheritdoc />
    public async Task<(TripNote Note, int NotificationsSent)> CreateTripNoteAsync(
        int tripInstanceId, string noteType, string body, bool resolveIncident,
        int? replacementShuttleId, int adminId)
    {
        // Validate body non-empty
        if (string.IsNullOrWhiteSpace(body))
            throw new InvalidOperationException("Trip note body cannot be empty.");

        var trip = await _db.TripInstances.FindAsync(tripInstanceId)
            ?? throw new KeyNotFoundException($"TripInstance {tripInstanceId} not found.");

        var note = new TripNote
        {
            TripInstanceId = tripInstanceId,
            NoteType = noteType,
            Body = body,
            CreatedByAdminId = adminId,
            CreatedAt = DateTime.UtcNow,
            IsResolutionAction = false
        };

        var notificationsSent = 0;

        // Resolution trigger logic
        var isResolutionType = noteType is "Incident" or "Replacement";
        if (resolveIncident && isResolutionType && trip.Status == "IncidentActive")
        {
            note.IsResolutionAction = true;

            // Find the open BreakdownLog
            var breakdownLog = await _db.BreakdownLogs
                .FirstOrDefaultAsync(bl =>
                    bl.TripInstanceId == tripInstanceId && bl.ResolvedAt == null);

            if (breakdownLog != null)
            {
                breakdownLog.ResolvedAt = DateTime.UtcNow;
                breakdownLog.ResolvedByAdminId = adminId;

                if (replacementShuttleId.HasValue)
                {
                    breakdownLog.ResolutionType = "Replaced";
                    breakdownLog.ReplacementShuttleId = replacementShuttleId.Value;

                    // Patch the allocation
                    var brokenAllocation = await _db.TripShuttleAllocations
                        .FirstOrDefaultAsync(a =>
                            a.TripInstanceId == tripInstanceId &&
                            a.ShuttleId == breakdownLog.ShuttleId);

                    if (brokenAllocation != null)
                    {
                        brokenAllocation.ShuttleId = replacementShuttleId.Value;
                        brokenAllocation.IsOperational = true;
                    }

                    trip.Status = "InTransit";
                }
                else
                {
                    breakdownLog.ResolutionType = "Cancelled";
                    // Trip stays as IncidentActive
                }
            }

            // Dispatch FCM to all confirmed passengers
            var summary = body.Length > 100 ? body[..100] : body;
            var title = "Trip Update";
            var message = $"{noteType}: {summary}";

            await _notificationService.DispatchToTripPassengersAsync(tripInstanceId, title, message);

            // Count confirmed bookings for response
            notificationsSent = await _db.Bookings
                .CountAsync(b =>
                    b.TripInstanceId == tripInstanceId &&
                    b.BookingStatus == "Confirmed");
        }

        _db.TripNotes.Add(note);
        await _db.SaveChangesAsync();

        return (note, notificationsSent);
    }

    /// <inheritdoc />
    public async Task<IList<TripNote>> GetTripNotesAsync(int tripInstanceId)
    {
        return await _db.TripNotes
            .Include(n => n.CreatedByAdmin)
            .Where(n => n.TripInstanceId == tripInstanceId)
            .OrderBy(n => n.CreatedAt)
            .ToListAsync();
    }
}
