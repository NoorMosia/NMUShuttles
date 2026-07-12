using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NmuShuttle.Api.DTOs;
using NmuShuttle.Api.Services;

namespace NmuShuttle.Api.Controllers;

/// <summary>
/// Manages shuttle breakdown flagging (Marshal) and resolution (Admin).
/// </summary>
[ApiController]
[Route("api/v1/trips/{tripId}")]
public class BreakdownController : ControllerBase
{
    private readonly IBreakdownService _breakdownService;

    public BreakdownController(IBreakdownService breakdownService)
    {
        _breakdownService = breakdownService;
    }

    /// <summary>
    /// Marshal flags a shuttle breakdown on a trip.
    /// Sets shuttle allocation as non-operational and creates a breakdown log.
    /// </summary>
    [HttpPost("breakdown")]
    [Authorize(Roles = "Marshal")]
    public async Task<IActionResult> FlagBreakdown(int tripId, [FromBody] FlagBreakdownDto dto)
    {
        var marshalId = GetUserIdFromClaims();
        if (marshalId is null)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = "Invalid or missing user identity claim.",
                Instance = HttpContext.Request.Path
            });
        }

        try
        {
            var breakdownLog = await _breakdownService.FlagBreakdownAsync(tripId, dto.ShuttleId, marshalId.Value);

            return StatusCode(StatusCodes.Status201Created, new
            {
                breakdownLog.BreakdownLogId,
                breakdownLog.TripInstanceId,
                breakdownLog.ShuttleId,
                breakdownLog.VendorCompanyName,
                breakdownLog.FlaggedAt,
                breakdownLog.FlaggedByMarshalId
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found",
                Detail = ex.Message,
                Instance = HttpContext.Request.Path
            });
        }
    }

    /// <summary>
    /// Admin resolves a breakdown by replacing the shuttle or cancelling.
    /// Dispatches notifications to all confirmed booking holders on the trip.
    /// </summary>
    [HttpPost("breakdown/{breakdownLogId}/resolve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ResolveBreakdown(int tripId, int breakdownLogId, [FromBody] ResolveBreakdownDto dto)
    {
        var adminId = GetUserIdFromClaims();
        if (adminId is null)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = "Invalid or missing user identity claim.",
                Instance = HttpContext.Request.Path
            });
        }

        // Validate resolution type
        if (dto.ResolutionType is not ("Replaced" or "Cancelled"))
        {
            return UnprocessableEntity(new ProblemDetails
            {
                Status = StatusCodes.Status422UnprocessableEntity,
                Title = "Validation Error",
                Detail = "ResolutionType must be 'Replaced' or 'Cancelled'.",
                Instance = HttpContext.Request.Path
            });
        }

        try
        {
            var breakdownLog = await _breakdownService.ResolveBreakdownAsync(
                breakdownLogId,
                dto.ResolutionType,
                dto.ReplacementShuttleId,
                adminId.Value);

            return Ok(new
            {
                breakdownLog.BreakdownLogId,
                breakdownLog.TripInstanceId,
                breakdownLog.ShuttleId,
                breakdownLog.VendorCompanyName,
                breakdownLog.ResolutionType,
                breakdownLog.ReplacementShuttleId,
                breakdownLog.ResolvedAt,
                breakdownLog.ResolvedByAdminId
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found",
                Detail = ex.Message,
                Instance = HttpContext.Request.Path
            });
        }
        catch (ArgumentException ex)
        {
            return UnprocessableEntity(new ProblemDetails
            {
                Status = StatusCodes.Status422UnprocessableEntity,
                Title = "Validation Error",
                Detail = ex.Message,
                Instance = HttpContext.Request.Path
            });
        }
    }

    private int? GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                          ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return null;

        return userId;
    }
}
