using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NmuShuttle.Api.Data;
using NmuShuttle.Api.DTOs;
using NmuShuttle.Api.DTOs.Admin;
using NmuShuttle.Api.Services;

namespace NmuShuttle.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/trips")]
[Authorize(Roles = "Admin")]
public class TripsAdminController : ControllerBase
{
    private readonly ITripStateMachineService _tripStateMachine;
    private readonly IConvoyOrderingService _convoyOrdering;
    private readonly ITripNoteService _tripNoteService;
    private readonly ShuttleDbContext _db;

    public TripsAdminController(
        ITripStateMachineService tripStateMachine,
        IConvoyOrderingService convoyOrdering,
        ITripNoteService tripNoteService,
        ShuttleDbContext db)
    {
        _tripStateMachine = tripStateMachine;
        _convoyOrdering = convoyOrdering;
        _tripNoteService = tripNoteService;
        _db = db;
    }

    /// <summary>
    /// Lists trip instances filterable by date and status.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ListTrips(
        [FromQuery] DateOnly? date,
        [FromQuery] string? status)
    {
        var query = _db.TripInstances
            .Include(t => t.ScheduleTime)
                .ThenInclude(st => st.ScheduleTimeCampuses)
                    .ThenInclude(stc => stc.Campus)
            .Include(t => t.Stop)
            .AsQueryable();

        if (date.HasValue)
            query = query.Where(t => t.TripDate == date.Value);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(t => t.Status == status);

        var trips = await query
            .OrderBy(t => t.TripDate)
            .ThenBy(t => t.ScheduleTime.DepartureTime)
            .ToListAsync();

        var response = trips.Select(t => new
        {
            t.TripInstanceId,
            t.TripDate,
            DepartureTime = t.ScheduleTime.DepartureTime,
            Direction = t.ScheduleTime.Direction,
            StopName = t.Stop.StopName,
            Campuses = t.ScheduleTime.ScheduleTimeCampuses
                .OrderBy(stc => stc.StopOrder)
                .Select(stc => new { stc.CampusId, stc.Campus.CampusName, stc.StopOrder })
                .ToList(),
            t.Status
        });

        return Ok(response);
    }

    /// <summary>
    /// Assigns a shuttle to a trip instance using accessible-first convoy ordering.
    /// </summary>
    [HttpPost("{id}/shuttles")]
    public async Task<IActionResult> AssignShuttle(int id, [FromBody] AssignShuttleDto dto)
    {
        try
        {
            var allocation = await _convoyOrdering.AssignShuttleToTripAsync(id, dto.ShuttleId);

            return CreatedAtAction(nameof(AssignShuttle), new { id }, new
            {
                allocation.AllocationId,
                allocation.ShuttleId,
                allocation.OrderPosition,
                allocation.IsOperational
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
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = ex.Message,
                Instance = HttpContext.Request.Path
            });
        }
    }

    /// <summary>
    /// Transitions a trip instance to a new status.
    /// Only valid transitions are allowed per the state machine rules.
    /// Returns HTTP 422 if the transition is invalid.
    /// </summary>
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] TripStatusUpdateDto dto)
    {
        // Validate transition before attempting it
        // We need to fetch the trip first to check the current status,
        // but TransitionAsync handles both lookup and validation.
        try
        {
            var updatedTrip = await _tripStateMachine.TransitionAsync(id, dto.Status);

            return Ok(new
            {
                updatedTrip.TripInstanceId,
                updatedTrip.Status,
                updatedTrip.LockedAt,
                updatedTrip.InTransitAt,
                updatedTrip.CompletedAt
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
        catch (InvalidOperationException ex)
        {
            return UnprocessableEntity(new ProblemDetails
            {
                Status = StatusCodes.Status422UnprocessableEntity,
                Title = "Invalid Status Transition",
                Detail = ex.Message,
                Instance = HttpContext.Request.Path
            });
        }
    }

    // ──────────────────────────────────────────────────────────────────
    // Trip Notes Endpoints
    // ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Create a trip note. May trigger incident resolution if resolveIncident = true.
    /// </summary>
    [HttpPost("{tripId}/notes")]
    public async Task<IActionResult> CreateTripNote(int tripId, [FromBody] CreateTripNoteDto dto)
    {
        // Extract admin ID from JWT claims
        var adminIdClaim = User.FindFirst("AdminId")?.Value
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (adminIdClaim == null || !int.TryParse(adminIdClaim, out var adminId))
            return Unauthorized(new { message = "Admin identity not found in token" });

        try
        {
            var (note, notificationsSent) = await _tripNoteService.CreateTripNoteAsync(
                tripId, dto.NoteType, dto.Body, dto.ResolveIncident,
                dto.ReplacementShuttleId, adminId);

            var admin = await _db.Admins.FindAsync(adminId);

            var response = new TripNoteResponseDto
            {
                TripNoteId = note.TripNoteId,
                TripInstanceId = note.TripInstanceId,
                NoteType = note.NoteType,
                Body = note.Body,
                IsResolutionAction = note.IsResolutionAction,
                CreatedByAdminId = note.CreatedByAdminId,
                CreatedByAdminName = admin != null ? $"{admin.FirstName} {admin.LastName}" : "Unknown",
                CreatedAt = note.CreatedAt,
                NotificationsSent = note.IsResolutionAction ? notificationsSent : null
            };

            return CreatedAtAction(nameof(GetTripNotes), new { tripId }, response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return UnprocessableEntity(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get all notes for a trip, ordered by CreatedAt ASC.
    /// </summary>
    [HttpGet("{tripId}/notes")]
    public async Task<IActionResult> GetTripNotes(int tripId)
    {
        var notes = await _tripNoteService.GetTripNotesAsync(tripId);

        var response = notes.Select(n => new TripNoteResponseDto
        {
            TripNoteId = n.TripNoteId,
            TripInstanceId = n.TripInstanceId,
            NoteType = n.NoteType,
            Body = n.Body,
            IsResolutionAction = n.IsResolutionAction,
            CreatedByAdminId = n.CreatedByAdminId,
            CreatedByAdminName = n.CreatedByAdmin != null
                ? $"{n.CreatedByAdmin.FirstName} {n.CreatedByAdmin.LastName}"
                : "Unknown",
            CreatedAt = n.CreatedAt
        });

        return Ok(response);
    }
}
