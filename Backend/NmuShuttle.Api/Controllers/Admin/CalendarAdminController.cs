using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NmuShuttle.Api.Data;
using NmuShuttle.Api.DTOs.Admin;
using NmuShuttle.Api.Models;

namespace NmuShuttle.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/calendar-exceptions")]
[Authorize(Roles = "Admin")]
public class CalendarAdminController : ControllerBase
{
    private readonly ShuttleDbContext _context;

    public CalendarAdminController(ShuttleDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// List all calendar exceptions, ordered by date.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var exceptions = await _context.CalendarExceptions
            .OrderBy(ce => ce.TargetDate)
            .Select(ce => new CalendarExceptionResponseDto
            {
                CalendarExceptionId = ce.CalendarExceptionId,
                TargetDate = ce.TargetDate,
                ExceptionType = ce.ExceptionType,
                Description = ce.Description
            })
            .ToListAsync();

        return Ok(exceptions);
    }

    /// <summary>
    /// Create a new calendar exception. Rejects duplicate TargetDate with 409.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCalendarExceptionDto dto)
    {
        // Reject duplicate TargetDate
        var duplicate = await _context.CalendarExceptions
            .AnyAsync(ce => ce.TargetDate == dto.TargetDate);
        if (duplicate)
            return Conflict(new { message = "A calendar exception for this date already exists" });

        var calendarException = new CalendarException
        {
            TargetDate = dto.TargetDate,
            ExceptionType = dto.ExceptionType,
            Description = dto.Description
        };

        _context.CalendarExceptions.Add(calendarException);
        await _context.SaveChangesAsync();

        var response = new CalendarExceptionResponseDto
        {
            CalendarExceptionId = calendarException.CalendarExceptionId,
            TargetDate = calendarException.TargetDate,
            ExceptionType = calendarException.ExceptionType,
            Description = calendarException.Description
        };

        return CreatedAtAction(nameof(GetById), new { id = calendarException.CalendarExceptionId }, response);
    }

    /// <summary>
    /// Get a single calendar exception by ID (used for CreatedAtAction routing).
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var calendarException = await _context.CalendarExceptions.FindAsync(id);
        if (calendarException == null)
            return NotFound(new { message = "Calendar exception not found" });

        return Ok(new CalendarExceptionResponseDto
        {
            CalendarExceptionId = calendarException.CalendarExceptionId,
            TargetDate = calendarException.TargetDate,
            ExceptionType = calendarException.ExceptionType,
            Description = calendarException.Description
        });
    }

    /// <summary>
    /// Delete a calendar exception.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var calendarException = await _context.CalendarExceptions.FindAsync(id);
        if (calendarException == null)
            return NotFound(new { message = "Calendar exception not found" });

        _context.CalendarExceptions.Remove(calendarException);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
