using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NmuShuttle.Api.Data;

namespace NmuShuttle.Api.Controllers.Admin;

/// <summary>
/// Admin endpoints for managing marshal stop/campus assignments.
/// </summary>
[ApiController]
[Route("api/v1/admin")]
[Authorize(Roles = "Admin")]
public class MarshalAssignmentAdminController : ControllerBase
{
    private readonly ShuttleDbContext _db;

    public MarshalAssignmentAdminController(ShuttleDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Get the marshal assigned to a specific stop.
    /// Returns null (204) if no marshal is assigned.
    /// </summary>
    [HttpGet("stops/{stopId:int}/marshal")]
    public async Task<IActionResult> GetStopMarshal(int stopId)
    {
        var marshal = await _db.Marshals
            .AsNoTracking()
            .Where(m => m.AssignedStopId == stopId && m.IsActive)
            .Select(m => new
            {
                m.MarshalId,
                m.FirstName,
                m.LastName,
                m.StaffNumber
            })
            .FirstOrDefaultAsync();

        if (marshal == null)
            return NoContent();

        return Ok(marshal);
    }

    /// <summary>
    /// Get all active marshals available to be assigned to a stop.
    /// Returns all active marshals with their current stop assignment (if any).
    /// </summary>
    [HttpGet("marshals/available-for-stop")]
    public async Task<IActionResult> GetMarshalsAvailableForStop()
    {
        var marshals = await _db.Marshals
            .AsNoTracking()
            .Where(m => m.IsActive)
            .Include(m => m.AssignedStop)
            .OrderBy(m => m.LastName)
            .ThenBy(m => m.FirstName)
            .Select(m => new
            {
                m.MarshalId,
                m.FirstName,
                m.LastName,
                m.StaffNumber,
                CurrentStopName = m.AssignedStop != null ? m.AssignedStop.StopName : null
            })
            .ToListAsync();

        return Ok(marshals);
    }

    /// <summary>
    /// Assign a marshal to a stop. Sets the marshal's AssignedStopId.
    /// </summary>
    [HttpPatch("marshals/{marshalId:int}/assign-stop")]
    public async Task<IActionResult> AssignMarshalToStop(int marshalId, [FromBody] AssignStopDto dto)
    {
        var marshal = await _db.Marshals.FindAsync(marshalId);
        if (marshal == null)
            return NotFound(new { message = "Marshal not found" });

        var stopExists = await _db.Stops.AnyAsync(s => s.StopId == dto.StopId);
        if (!stopExists)
            return NotFound(new { message = "Stop not found" });

        marshal.AssignedStopId = dto.StopId;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Marshal assigned to stop", marshalId, stopId = dto.StopId });
    }

    /// <summary>
    /// Unassign a marshal from their stop. Sets AssignedStopId to null.
    /// </summary>
    [HttpPatch("marshals/{marshalId:int}/unassign-stop")]
    public async Task<IActionResult> UnassignMarshalFromStop(int marshalId)
    {
        var marshal = await _db.Marshals.FindAsync(marshalId);
        if (marshal == null)
            return NotFound(new { message = "Marshal not found" });

        marshal.AssignedStopId = null;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Marshal unassigned from stop", marshalId });
    }
}

public class AssignStopDto
{
    public int StopId { get; set; }
}
