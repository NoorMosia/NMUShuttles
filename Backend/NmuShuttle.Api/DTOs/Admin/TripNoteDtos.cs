namespace NmuShuttle.Api.DTOs.Admin;

public class CreateTripNoteDto
{
    public string NoteType { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool ResolveIncident { get; set; }
    public int? ReplacementShuttleId { get; set; }
}

public class TripNoteResponseDto
{
    public int TripNoteId { get; set; }
    public int TripInstanceId { get; set; }
    public string NoteType { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsResolutionAction { get; set; }
    public int CreatedByAdminId { get; set; }
    public string CreatedByAdminName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int? NotificationsSent { get; set; }
}
