using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NmuShuttle.Api.Data;
using NmuShuttle.Api.DTOs.Admin;
using NmuShuttle.Api.Models;

namespace NmuShuttle.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/reports")]
[Authorize(Roles = "Admin")]
public class ReportsAdminController : ControllerBase
{
    private readonly ShuttleDbContext _db;

    private static readonly HashSet<string> ValidStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Submitted",
        "UnderReview",
        "Resolved"
    };

    public ReportsAdminController(ShuttleDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Lists all reports for admin review.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ListReports()
    {
        var reports = await _db.Reports
            .Include(r => r.Student)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                r.ReportId,
                r.Category,
                r.StudentId,
                StudentNumber = r.Student.StudentNumber,
                SubmissionDate = r.CreatedAt,
                r.Status
            })
            .ToListAsync();

        return Ok(reports);
    }

    /// <summary>
    /// Gets full report details including description, admin notes, and linked trip.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetReport(int id)
    {
        var report = await _db.Reports
            .Include(r => r.Student)
            .Include(r => r.Booking)
                .ThenInclude(b => b!.Shuttle)
                    .ThenInclude(s => s!.Vendor)
            .Include(r => r.TripInstance)
                .ThenInclude(t => t!.ScheduleTime)
            .Include(r => r.TripInstance)
                .ThenInclude(t => t!.Stop)
            .FirstOrDefaultAsync(r => r.ReportId == id);

        if (report is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found",
                Detail = $"Report with ID {id} not found.",
                Instance = HttpContext.Request.Path
            });
        }

        var response = MapToDto(report);
        return Ok(response);
    }

    /// <summary>
    /// Updates a report's status and/or admin notes.
    /// </summary>
    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateReport(int id, [FromBody] UpdateReportDto dto)
    {
        var report = await _db.Reports
            .Include(r => r.Student)
            .Include(r => r.Booking)
                .ThenInclude(b => b!.Shuttle)
                    .ThenInclude(s => s!.Vendor)
            .Include(r => r.TripInstance)
                .ThenInclude(t => t!.ScheduleTime)
            .Include(r => r.TripInstance)
                .ThenInclude(t => t!.Stop)
            .FirstOrDefaultAsync(r => r.ReportId == id);

        if (report is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found",
                Detail = $"Report with ID {id} not found.",
                Instance = HttpContext.Request.Path
            });
        }

        if (dto.Status is not null)
        {
            if (!ValidStatuses.Contains(dto.Status))
            {
                return UnprocessableEntity(new ProblemDetails
                {
                    Status = StatusCodes.Status422UnprocessableEntity,
                    Title = "Invalid Status",
                    Detail = $"Status must be one of: {string.Join(", ", ValidStatuses)}.",
                    Instance = HttpContext.Request.Path
                });
            }

            report.Status = dto.Status;
        }

        if (dto.AdminNotes is not null)
        {
            report.AdminNotes = dto.AdminNotes;
        }

        report.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var response = MapToDto(report);
        return Ok(response);
    }

    private static AdminReportResponseDto MapToDto(Report report)
    {
        var dto = new AdminReportResponseDto
        {
            ReportId = report.ReportId,
            Category = report.Category,
            Description = report.Description,
            Status = report.Status,
            StudentId = report.StudentId,
            StudentNumber = report.Student.StudentNumber,
            StudentName = $"{report.Student.FirstName} {report.Student.LastName}",
            TripInstanceId = report.TripInstanceId,
            BookingId = report.BookingId,
            AdminNotes = report.AdminNotes,
            SubmissionDate = report.CreatedAt,
            UpdatedAt = report.UpdatedAt
        };

        if (report.TripInstance is not null)
        {
            dto.TripDetails = new TripDetailsDto
            {
                TripInstanceId = report.TripInstance.TripInstanceId,
                TripDate = report.TripInstance.TripDate.ToString("yyyy-MM-dd"),
                DepartureTime = report.TripInstance.ScheduleTime.DepartureTime.ToString("HH:mm"),
                Direction = report.TripInstance.ScheduleTime.Direction,
                StopName = report.TripInstance.Stop.StopName,
                TripStatus = report.TripInstance.Status,
                ShuttlePlate = report.Booking?.Shuttle?.LicensePlate,
                VendorName = report.Booking?.Shuttle?.Vendor?.CompanyName
            };
        }

        return dto;
    }
}
