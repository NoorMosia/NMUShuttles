using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NmuShuttle.Api.Data;
using NmuShuttle.Api.DTOs;

namespace NmuShuttle.Api.Controllers;

/// <summary>
/// Endpoints for viewing and managing notifications. Serves both Students and
/// Marshals — each sees only their own notifications, scoped by the JWT role claim.
/// </summary>
[ApiController]
[Route("api/v1/notifications")]
[Authorize(Roles = "Student,Marshal")]
public class NotificationsController : ControllerBase
{
    private readonly ShuttleDbContext _dbContext;

    public NotificationsController(ShuttleDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Returns all notifications for the authenticated user, ordered by CreatedAt DESC.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetNotifications()
    {
        var owner = GetOwner();
        if (owner is null)
            return Unauthorized();

        var (ownerId, ownerType) = owner.Value;

        var notifications = await _dbContext.Notifications
            .Where(n => n.OwnerId == ownerId && n.OwnerType == ownerType)
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
    /// Marks a specific notification as read for the authenticated user.
    /// </summary>
    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var owner = GetOwner();
        if (owner is null)
            return Unauthorized();

        var (ownerId, ownerType) = owner.Value;

        var notification = await _dbContext.Notifications
            .FirstOrDefaultAsync(n =>
                n.NotificationId == id &&
                n.OwnerId == ownerId &&
                n.OwnerType == ownerType);

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

    /// <summary>
    /// Resolves the current user's id and owner type ("Student"/"Marshal") from JWT claims.
    /// </summary>
    private (int OwnerId, string OwnerType)? GetOwner()
    {
        var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                          ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var ownerId))
            return null;

        var ownerType = User.FindFirstValue(ClaimTypes.Role);
        if (ownerType is not ("Student" or "Marshal"))
            return null;

        return (ownerId, ownerType);
    }
}
