using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NmuShuttle.Api.Data;
using NmuShuttle.Api.DTOs.Admin;
using NmuShuttle.Api.Models;

namespace NmuShuttle.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/schedule-times")]
[Authorize(Roles = "Admin")]
public class ScheduleAdminController : ControllerBase
{
    private readonly ShuttleDbContext _context;

    public ScheduleAdminController(ShuttleDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// List all schedule times.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var scheduleTimes = await _context.ScheduleTimes
            .Select(st => new ScheduleTimeResponseDto
            {
                ScheduleTimeId = st.ScheduleTimeId,
                DepartureTime = st.DepartureTime,
                Direction = st.Direction,
                ScheduleType = st.ScheduleType,
                CampusId = st.CampusId,
                StopId = st.StopId
            })
            .ToListAsync();

        return Ok(scheduleTimes);
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
            CampusId = dto.CampusId,
            StopId = dto.StopId
        };

        _context.ScheduleTimes.Add(scheduleTime);
        await _context.SaveChangesAsync();

        var response = new ScheduleTimeResponseDto
        {
            ScheduleTimeId = scheduleTime.ScheduleTimeId,
            DepartureTime = scheduleTime.DepartureTime,
            Direction = scheduleTime.Direction,
            ScheduleType = scheduleTime.ScheduleType,
            CampusId = scheduleTime.CampusId,
            StopId = scheduleTime.StopId
        };

        return CreatedAtAction(nameof(GetById), new { id = scheduleTime.ScheduleTimeId }, response);
    }

    /// <summary>
    /// Get a single schedule time by ID (used for CreatedAtAction routing).
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var scheduleTime = await _context.ScheduleTimes.FindAsync(id);
        if (scheduleTime == null)
            return NotFound(new { message = "Schedule time not found" });

        return Ok(new ScheduleTimeResponseDto
        {
            ScheduleTimeId = scheduleTime.ScheduleTimeId,
            DepartureTime = scheduleTime.DepartureTime,
            Direction = scheduleTime.Direction,
            ScheduleType = scheduleTime.ScheduleType,
            CampusId = scheduleTime.CampusId,
            StopId = scheduleTime.StopId
        });
    }

    /// <summary>
    /// Update an existing schedule time.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateScheduleTimeDto dto)
    {
        var scheduleTime = await _context.ScheduleTimes.FindAsync(id);
        if (scheduleTime == null)
            return NotFound(new { message = "Schedule time not found" });

        scheduleTime.DepartureTime = dto.DepartureTime;
        scheduleTime.Direction = dto.Direction;
        scheduleTime.ScheduleType = dto.ScheduleType;
        scheduleTime.CampusId = dto.CampusId;
        scheduleTime.StopId = dto.StopId;

        await _context.SaveChangesAsync();

        return Ok(new ScheduleTimeResponseDto
        {
            ScheduleTimeId = scheduleTime.ScheduleTimeId,
            DepartureTime = scheduleTime.DepartureTime,
            Direction = scheduleTime.Direction,
            ScheduleType = scheduleTime.ScheduleType,
            CampusId = scheduleTime.CampusId,
            StopId = scheduleTime.StopId
        });
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
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var hasFutureTrips = await _context.TripInstances
            .AnyAsync(t => t.ScheduleTimeId == id && t.TripDate >= today);
        if (hasFutureTrips)
            return Conflict(new { message = "Cannot delete schedule time: it is referenced by future trip instances" });

        _context.ScheduleTimes.Remove(scheduleTime);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
