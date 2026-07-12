using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NmuShuttle.Api.Data;
using NmuShuttle.Api.DTOs.Admin;
using NmuShuttle.Api.Models;

namespace NmuShuttle.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/priority-schedules")]
[Authorize(Roles = "Admin")]
public class PriorityAdminController : ControllerBase
{
    private readonly ShuttleDbContext _context;

    public PriorityAdminController(ShuttleDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// List all priority schedules.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var schedules = await _context.PrioritySchedules
            .Select(ps => new PriorityScheduleResponseDto
            {
                PriorityScheduleId = ps.PriorityScheduleId,
                StudentId = ps.StudentId,
                StopId = ps.StopId,
                ScheduleTimeId = ps.ScheduleTimeId,
                DayOfWeek = ps.DayOfWeek,
                IsActive = ps.IsActive,
                CreatedAt = ps.CreatedAt
            })
            .ToListAsync();

        return Ok(schedules);
    }

    /// <summary>
    /// Create a new priority schedule. Auto-sets Student.IsPriorityUser=true if not already.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePriorityScheduleDto dto)
    {
        // Validate that the student exists
        var student = await _context.Students.FindAsync(dto.StudentId);
        if (student == null)
            return NotFound(new { message = "Student not found" });

        // Validate that the stop exists
        var stopExists = await _context.Stops.AnyAsync(s => s.StopId == dto.StopId);
        if (!stopExists)
            return NotFound(new { message = "Stop not found" });

        // Validate that the schedule time exists
        var scheduleTimeExists = await _context.ScheduleTimes.AnyAsync(st => st.ScheduleTimeId == dto.ScheduleTimeId);
        if (!scheduleTimeExists)
            return NotFound(new { message = "Schedule time not found" });

        var prioritySchedule = new PrioritySchedule
        {
            StudentId = dto.StudentId,
            StopId = dto.StopId,
            ScheduleTimeId = dto.ScheduleTimeId,
            DayOfWeek = dto.DayOfWeek,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.PrioritySchedules.Add(prioritySchedule);

        // Auto-set Student.IsPriorityUser = true if not already
        if (!student.IsPriorityUser)
        {
            student.IsPriorityUser = true;
        }

        await _context.SaveChangesAsync();

        var response = new PriorityScheduleResponseDto
        {
            PriorityScheduleId = prioritySchedule.PriorityScheduleId,
            StudentId = prioritySchedule.StudentId,
            StopId = prioritySchedule.StopId,
            ScheduleTimeId = prioritySchedule.ScheduleTimeId,
            DayOfWeek = prioritySchedule.DayOfWeek,
            IsActive = prioritySchedule.IsActive,
            CreatedAt = prioritySchedule.CreatedAt
        };

        return CreatedAtAction(nameof(GetById), new { id = prioritySchedule.PriorityScheduleId }, response);
    }

    /// <summary>
    /// Get a single priority schedule by ID (used for CreatedAtAction routing).
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var ps = await _context.PrioritySchedules.FindAsync(id);
        if (ps == null)
            return NotFound(new { message = "Priority schedule not found" });

        return Ok(new PriorityScheduleResponseDto
        {
            PriorityScheduleId = ps.PriorityScheduleId,
            StudentId = ps.StudentId,
            StopId = ps.StopId,
            ScheduleTimeId = ps.ScheduleTimeId,
            DayOfWeek = ps.DayOfWeek,
            IsActive = ps.IsActive,
            CreatedAt = ps.CreatedAt
        });
    }

    /// <summary>
    /// Deactivate a priority schedule (set IsActive=false).
    /// </summary>
    [HttpPatch("{id}")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var ps = await _context.PrioritySchedules.FindAsync(id);
        if (ps == null)
            return NotFound(new { message = "Priority schedule not found" });

        ps.IsActive = false;
        await _context.SaveChangesAsync();

        return Ok(new PriorityScheduleResponseDto
        {
            PriorityScheduleId = ps.PriorityScheduleId,
            StudentId = ps.StudentId,
            StopId = ps.StopId,
            ScheduleTimeId = ps.ScheduleTimeId,
            DayOfWeek = ps.DayOfWeek,
            IsActive = ps.IsActive,
            CreatedAt = ps.CreatedAt
        });
    }
}
