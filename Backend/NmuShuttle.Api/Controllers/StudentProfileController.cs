using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NmuShuttle.Api.Data;
using NmuShuttle.Api.DTOs;

namespace NmuShuttle.Api.Controllers;

/// <summary>
/// Endpoint for the authenticated student to retrieve their own profile.
/// </summary>
[ApiController]
[Route("api/v1/students")]
[Authorize(Roles = "Student")]
public class StudentProfileController : ControllerBase
{
    private readonly ShuttleDbContext _db;

    public StudentProfileController(ShuttleDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Returns the authenticated student's profile including strike point stats.
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(StudentProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyProfile()
    {
        var studentId = GetStudentId();

        var student = await _db.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.StudentId == studentId);

        if (student == null)
            return NotFound();

        // Count no-shows and cancellations from bookings
        var noShowCount = await _db.Bookings
            .CountAsync(b => b.StudentId == studentId && b.BoardedStatus == "NoShow");

        var cancelledCount = await _db.Bookings
            .CountAsync(b => b.StudentId == studentId && b.BookingStatus == "Cancelled");

        // Strike points: no-show = 2 points, cancellation = 1 point, max 6
        var strikePoints = Math.Min((noShowCount * 2) + cancelledCount, 6);

        return Ok(new StudentProfileDto
        {
            StudentId = student.StudentId,
            StudentNumber = student.StudentNumber,
            FirstName = student.FirstName,
            LastName = student.LastName,
            Email = student.UniversityEmail,
            IsPriorityUser = student.IsPriorityUser,
            NoShowCount = noShowCount,
            CancelledCount = cancelledCount,
            StrikePoints = strikePoints,
        });
    }

    private int GetStudentId()
    {
        var subClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                       ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.Parse(subClaim!);
    }
}
