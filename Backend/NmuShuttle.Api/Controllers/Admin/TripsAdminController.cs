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
    private readonly ShuttleDbContext _db;

    public TripsAdminController(
        ITripStateMachineService tripStateMachine,
        IConvoyOrderingService convoyOrdering,
        ShuttleDbContext db)
    {
        _tripStateMachine = tripStateMachine;
        _convoyOrdering = convoyOrdering;
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
                .ThenInclude(st => st.Campus)
            .Include(t => t.Stop)
            .AsQueryable();

        if (date.HasValue)
            query = query.Where(t => t.TripDate == date.Value);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(t => t.Status == status);

        var trips = await query
            .OrderBy(t => t.TripDate)
            .ThenBy(t => t.ScheduleTime.DepartureTime)
            .Select(t => new
            {
                t.TripInstanceId,
                t.TripDate,
                DepartureTime = t.ScheduleTime.DepartureTime,
                Direction = t.ScheduleTime.Direction,
                StopName = t.Stop.StopName,
                CampusName = t.ScheduleTime.Campus.CampusName,
                t.Status
            })
            .ToListAsync();

        return Ok(trips);
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
}
