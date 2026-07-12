using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NmuShuttle.Api.Data;
using NmuShuttle.Api.DTOs;

namespace NmuShuttle.Api.Controllers;

/// <summary>
/// Student-facing endpoints for viewing and managing notifications.
/// </summary>
[ApiController]
[Route("api/v1/notifications")]
[Authorize(Roles = "Student")]
public class NotificationsController : ControllerBase
{
    private readonly ShuttleDbContext _dbContext;

    public NotificationsController(ShuttleDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Returns all notifications for the authenticated student, ordered by CreatedAt DESC.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetNotifications()
    {
        var studentId = GetStudentId();
        if (studentId is null)
            return Unauthorized();

        var notifications = await _dbContext.Notifications
            .Where(n => n.StudentId == studentId.Value)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationResponseDto
            {
                NotificationId = n.NotificationId,
                Title = n.Title,
                Message = n.Message,
                IsRead = n.IsRead,
                RelatedTripInstanceId = n.RelatedTripInstanceId,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync();

        return Ok(notifications);
    }

    /// <summary>
    /// Marks a specific notification as read for the authenticated student.
    /// </summary>
    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var studentId = GetStudentId();
        if (studentId is null)
            return Unauthorized();

        var notification = await _dbContext.Notifications
            .FirstOrDefaultAsync(n => n.NotificationId == id && n.StudentId == studentId.Value);

        if (notification is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found",
                Detail = "Notification not found.",
                Instance = HttpContext.Request.Path
            });
        }

        notification.IsRead = true;
        await _dbContext.SaveChangesAsync();

        return Ok(new { Message = "Notification marked as read." });
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
