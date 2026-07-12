using System.ComponentModel.DataAnnotations;

namespace NmuShuttle.Api.DTOs.Admin;

/// <summary>
/// Request body for updating a report's status and/or admin notes.
/// </summary>
public class UpdateReportDto
{
    [MaxLength(20)]
    public string? Status { get; set; }

    [MaxLength(2000)]
    public string? AdminNotes { get; set; }
}
