using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NmuShuttle.Api.Data;
using NmuShuttle.Api.DTOs.Admin;
using NmuShuttle.Api.Infrastructure;
using NmuShuttle.Api.Models;
using NmuShuttle.Api.Services;

namespace NmuShuttle.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/schedule-times")]
[Authorize(Roles = "Admin")]
public class ScheduleAdminController : ControllerBase
{
    private readonly ShuttleDbContext _context;
    private readonly IScheduleTimeCampusService _campusService;

    public ScheduleAdminController(ShuttleDbContext context, IScheduleTimeCampusService campusService)
    {
        _context = context;
        _campusService = campusService;
    }

    /// <summary>
    /// List all schedule times.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var scheduleTimes = await _context.ScheduleTimes
            .Include(st => st.ScheduleTimeCampuses)
                .ThenInclude(stc => stc.Campus)
            .ToListAsync();

        var response = scheduleTimes.Select(st => MapToResponseDto(st)).ToList();

        return Ok(response);
    }

    /// <summary>
    /// Create a new schedule time.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateScheduleTimeDto dto)
    {
        var scheduleTime = new ScheduleTime
        {
            DepartureTime = dto.DepartureTime,
            Direction = dto.Direction,
            ScheduleType = dto.ScheduleType,
            StopId = dto.StopId,
            RequiresScanning = dto.RequiresScanning
        };

        _context.ScheduleTimes.Add(scheduleTime);
        await _context.SaveChangesAsync();

        // Populate ScheduleTimeCampus records
        var campusIds = dto.CampusIds ?? new List<int>();
        for (int i = 0; i < campusIds.Count; i++)
        {
            _context.ScheduleTimeCampuses.Add(new Models.ScheduleTimeCampus
            {
                ScheduleTimeId = scheduleTime.ScheduleTimeId,
                CampusId = campusIds[i],
                StopOrder = i + 1
            });
        }
        await _context.SaveChangesAsync();

        // Reload with campuses for response
        var loaded = await _context.ScheduleTimes
            .Include(st => st.ScheduleTimeCampuses)
                .ThenInclude(stc => stc.Campus)
            .FirstAsync(st => st.ScheduleTimeId == scheduleTime.ScheduleTimeId);

        return CreatedAtAction(nameof(GetById), new { id = scheduleTime.ScheduleTimeId }, MapToResponseDto(loaded));
    }

    /// <summary>
    /// Get a single schedule time by ID (used for CreatedAtAction routing).
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var scheduleTime = await _context.ScheduleTimes
            .Include(st => st.ScheduleTimeCampuses)
                .ThenInclude(stc => stc.Campus)
            .FirstOrDefaultAsync(st => st.ScheduleTimeId == id);

        if (scheduleTime == null)
            return NotFound(new { message = "Schedule time not found" });

        return Ok(MapToResponseDto(scheduleTime));
    }

    /// <summary>
    /// Update an existing schedule time.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateScheduleTimeDto dto)
    {
        var scheduleTime = await _context.ScheduleTimes
            .Include(st => st.ScheduleTimeCampuses)
            .FirstOrDefaultAsync(st => st.ScheduleTimeId == id);

        if (scheduleTime == null)
            return NotFound(new { message = "Schedule time not found" });

        scheduleTime.DepartureTime = dto.DepartureTime;
        scheduleTime.Direction = dto.Direction;
        scheduleTime.ScheduleType = dto.ScheduleType;
        scheduleTime.StopId = dto.StopId;
        scheduleTime.RequiresScanning = dto.RequiresScanning;

        // Update ScheduleTimeCampus records if CampusIds provided
        var campusIds = dto.CampusIds;
        if (campusIds != null)
        {
            _context.ScheduleTimeCampuses.RemoveRange(scheduleTime.ScheduleTimeCampuses);
            for (int i = 0; i < campusIds.Count; i++)
            {
                _context.ScheduleTimeCampuses.Add(new Models.ScheduleTimeCampus
                {
                    ScheduleTimeId = id,
                    CampusId = campusIds[i],
                    StopOrder = i + 1
                });
            }
        }

        await _context.SaveChangesAsync();

        // Reload with campuses for response
        var loaded = await _context.ScheduleTimes
            .Include(st => st.ScheduleTimeCampuses)
                .ThenInclude(stc => stc.Campus)
            .FirstAsync(st => st.ScheduleTimeId == id);

        return Ok(MapToResponseDto(loaded));
    }

    /// <summary>
    /// Delete a schedule time. Rejects if future TripInstances reference it (HTTP 409).
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var scheduleTime = await _context.ScheduleTimes.FindAsync(id);
        if (scheduleTime == null)
            return NotFound(new { message = "Schedule time not found" });

        // Reject if future TripInstances reference this schedule time
        var today = SastClock.Today;
        var hasFutureTrips = await _context.TripInstances
            .AnyAsync(t => t.ScheduleTimeId == id && t.TripDate >= today);
        if (hasFutureTrips)
            return Conflict(new { message = "Cannot delete schedule time: it is referenced by future trip instances" });

        _context.ScheduleTimes.Remove(scheduleTime);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // ──────────────────────────────────────────────────────────────────
    // Campus Route Endpoints (ScheduleTimeCampus management)
    // ──────────────────────────────────────────────────────────────────

    private static ScheduleTimeResponseDto MapToResponseDto(Models.ScheduleTime st) => new()
    {
        ScheduleTimeId = st.ScheduleTimeId,
        DepartureTime = st.DepartureTime,
        Direction = st.Direction,
        ScheduleType = st.ScheduleType,
        StopId = st.StopId,
        RequiresScanning = st.RequiresScanning,
        Campuses = st.ScheduleTimeCampuses
            .OrderBy(stc => stc.StopOrder)
            .Select(stc => new ScheduleTimeCampusItemDto
            {
                CampusId = stc.CampusId,
                CampusName = stc.Campus?.CampusName ?? "",
                StopOrder = stc.StopOrder
            }).ToList()
    };

    /// <summary>
    /// Get the ordered campus sequence for a schedule time's route.
    /// </summary>
    [HttpGet("{id}/campuses")]
    public async Task<IActionResult> GetCampuses(int id)
    {
        var campuses = await _campusService.GetRouteCampusesAsync(id);

        var response = campuses.Select(stc => new
        {
            stc.ScheduleTimeCampusId,
            stc.ScheduleTimeId,
            stc.CampusId,
            CampusName = stc.Campus.CampusName,
            stc.StopOrder
        });

        return Ok(response);
    }

    /// <summary>
    /// Add a campus to a route.
    /// </summary>
    [HttpPost("{id}/campuses")]
    public async Task<IActionResult> AddCampus(int id, [FromBody] AddCampusToRouteDto dto)
    {
        try
        {
            var record = await _campusService.AddCampusToRouteAsync(id, dto.CampusId, dto.StopOrder);

            return CreatedAtAction(nameof(GetCampuses), new { id }, new
            {
                record.ScheduleTimeCampusId,
                record.ScheduleTimeId,
                record.CampusId,
                record.StopOrder
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Remove a campus from a route.
    /// </summary>
    [HttpDelete("{id}/campuses/{campusId}")]
    public async Task<IActionResult> RemoveCampus(int id, int campusId)
    {
        try
        {
            await _campusService.RemoveCampusFromRouteAsync(id, campusId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Reorder campuses on a route.
    /// </summary>
    [HttpPatch("{id}/campuses/reorder")]
    public async Task<IActionResult> ReorderCampuses(int id, [FromBody] ReorderCampusesDto dto)
    {
        try
        {
            await _campusService.ReorderRouteAsync(id, dto.OrderedCampusIds);
            var campuses = await _campusService.GetRouteCampusesAsync(id);

            var response = campuses.Select(stc => new
            {
                stc.ScheduleTimeCampusId,
                stc.ScheduleTimeId,
                stc.CampusId,
                CampusName = stc.Campus.CampusName,
                stc.StopOrder
            });

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return UnprocessableEntity(new { message = ex.Message });
        }
    }
}
