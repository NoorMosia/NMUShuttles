namespace NmuShuttle.Api.DTOs;

/// <summary>
/// Response DTO for the authenticated student's profile information.
/// </summary>
public class StudentProfileDto
{
    public int StudentId { get; set; }
    public string StudentNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsPriorityUser { get; set; }
    public int NoShowCount { get; set; }
    public int CancelledCount { get; set; }
    public int StrikePoints { get; set; }
}
