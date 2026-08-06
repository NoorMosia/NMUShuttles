using NmuShuttle.Api.Models;

namespace NmuShuttle.Api.Services;

/// <summary>
/// Manages TripNote creation, including incident resolution trigger logic.
/// </summary>
public interface ITripNoteService
{
    /// <summary>
    /// Creates a trip note. If resolveIncident is true and conditions are met,
    /// triggers BreakdownLog resolution and FCM notifications.
    /// </summary>
    /// <returns>The created TripNote and the number of notifications sent (0 if not a resolution).</returns>
    Task<(TripNote Note, int NotificationsSent)> CreateTripNoteAsync(
        int tripInstanceId, string noteType, string body, bool resolveIncident,
        int? replacementShuttleId, int adminId);

    /// <summary>
    /// Returns all notes for a trip, ordered by CreatedAt ASC.
    /// </summary>
    Task<IList<TripNote>> GetTripNotesAsync(int tripInstanceId);
}
