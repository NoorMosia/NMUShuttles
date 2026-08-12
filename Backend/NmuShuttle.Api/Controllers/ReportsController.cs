using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NmuShuttle.Api.Data;
using NmuShuttle.Api.DTOs;
using NmuShuttle.Api.Models;

namespace NmuShuttle.Api.Controllers;

/// <summary>
/// Student-facing endpoints for creating and viewing reports.
/// </summary>
[ApiController]
[Route("api/v1/reports")]
[Authorize(Roles = "Student")]
public class ReportsController : ControllerBase
{
    private static readonly HashSet<string> ValidCategories = new(StringComparer.Ordinal)
    {
        "DriverBehavior",
        "TimetableIssue",
        "VehicleQuality",
        "Other"
    };

    private readonly ShuttleDbContext _dbContext;

    public ReportsController(ShuttleDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Creates a new report for the authenticated student.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateReport([FromBody] CreateReportDto dto)
    {
        var studentId = GetStudentId();
        if (studentId is null)
            return Unauthorized();

        // Validate Description is non-empty
        if (string.IsNullOrWhiteSpace(dto.Description))
        {
            return UnprocessableEntity(new ProblemDetails
            {
                Status = StatusCodes.Status422UnprocessableEntity,
                Title = "Validation Error",
                Detail = "Description is required and cannot be empty.",
                Instance = HttpContext.Request.Path
            });
        }

        // Validate Category
        if (!ValidCategories.Contains(dto.Category))
        {
            return UnprocessableEntity(new ProblemDetails
            {
                Status = StatusCodes.Status422UnprocessableEntity,
                Title = "Validation Error",
                Detail = "Category must be one of: DriverBehavior, TimetableIssue, VehicleQuality, Other.",
                Instance = HttpContext.Request.Path
            });
        }

        // Look up the booking and validate it belongs to this student
        var booking = await _dbContext.Bookings
            .FirstOrDefaultAsync(b => b.BookingId == dto.BookingId && b.StudentId == studentId.Value);

        if (booking is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found",
                Detail = "Booking not found or does not belong to this student.",
                Instance = HttpContext.Request.Path
            });
        }

        // Prevent duplicate reports on the same booking
        var existingReport = await _dbContext.Reports
            .AnyAsync(r => r.BookingId == dto.BookingId && r.StudentId == studentId.Value);

        if (existingReport)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Duplicate Report",
                Detail = "You have already submitted a report for this booking.",
                Instance = HttpContext.Request.Path
            });
        }

        var report = new Report
        {
            StudentId = studentId.Value,
            Category = dto.Category,
            Description = dto.Description,
            Status = "Submitted",
            BookingId = booking.BookingId,
            TripInstanceId = booking.TripInstanceId,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Reports.Add(report);
        await _dbContext.SaveChangesAsync();

        var response = new ReportResponseDto
        {
            ReportId = report.ReportId,
            Category = report.Category,
            Description = report.Description,
            Status = report.Status,
            AdminNotes = report.AdminNotes,
            TripInstanceId = report.TripInstanceId,
            BookingId = report.BookingId,
            CreatedAt = report.CreatedAt,
            UpdatedAt = report.UpdatedAt
        };

        return Created($"/api/v1/reports/{report.ReportId}", response);
    }

    /// <summary>
    /// Returns all reports for the authenticated student, ordered by CreatedAt DESC.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetReports()
    {
        var studentId = GetStudentId();
        if (studentId is null)
            return Unauthorized();

        var reports = await _dbContext.Reports
            .Where(r => r.StudentId == studentId.Value)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReportResponseDto
            {
                ReportId = r.ReportId,
                Category = r.Category,
                Description = r.Description,
                Status = r.Status,
                AdminNotes = r.AdminNotes,
                TripInstanceId = r.TripInstanceId,
                BookingId = r.BookingId,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            })
            .ToListAsync();

        return Ok(reports);
    }

    private int? GetStudentId()
    {
        var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                          ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var studentId))
            return null;

        return studentId;
    }
}
