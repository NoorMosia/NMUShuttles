using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NmuShuttle.Api.Data;
using NmuShuttle.Api.DTOs.Admin;
using NmuShuttle.Api.Models;
using NmuShuttle.Api.Services;

namespace NmuShuttle.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/calendar-exceptions")]
[Authorize(Roles = "Admin")]
public class CalendarAdminController : ControllerBase
{
    private readonly ShuttleDbContext _context;
    private readonly INotificationService _notificationService;

    /// <summary>
    /// Allowed ExceptionType values. Must stay in sync with the DB CHECK
    /// constraint CK_CalendarExceptions_ExceptionType.
    /// </summary>
    private static readonly HashSet<string> AllowedExceptionTypes = new()
    {
        "PublicHoliday",
        "UniversityRecess"
    };

    public CalendarAdminController(ShuttleDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
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
        // Validate ExceptionType against the allowed set (matches the DB CHECK
        // constraint CK_CalendarExceptions_ExceptionType). Return a clean 422
        // rather than letting the INSERT fail with a 500 at the database.
        if (!AllowedExceptionTypes.Contains(dto.ExceptionType))
            return UnprocessableEntity(new
            {
                message =
                    $"Invalid ExceptionType '{dto.ExceptionType}'. Allowed values: {string.Join(", ", AllowedExceptionTypes)}."
            });

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

        // Persist the exception AND retroactively cancel any already-generated
        // trips/bookings for this date, atomically. EnableRetryOnFailure is on,
        // so the transaction must run inside the execution strategy.
        var affectedStudentIds = new List<int>();
        var strategy = _context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            _context.CalendarExceptions.Add(calendarException);
            await _context.SaveChangesAsync();

            affectedStudentIds = await CancelTripsForDateAsync(dto.TargetDate);

            await transaction.CommitAsync();
        });

        // Build the human-readable labels used in all notifications below.
        var typeLabel = calendarException.ExceptionType == "PublicHoliday"
            ? "Public holiday"
            : "University recess";
        var dateLabel = calendarException.TargetDate.ToString("dddd, dd MMM yyyy");
        var descSuffix = string.IsNullOrWhiteSpace(calendarException.Description)
            ? ""
            : $" ({calendarException.Description})";

        // Notify all active marshals that a new calendar exception affects scheduling.
        var marshalBody =
            $"{typeLabel} added for {dateLabel}{descSuffix}. No shuttles will run on this date.";
        await _notificationService.DispatchToAllMarshalsAsync(
            "Schedule exception added",
            marshalBody);

        // Notify every student whose booking was just cancelled by this exception.
        // Their bookings are now 'Cancelled', so DispatchToTripPassengersAsync would
        // no longer match them — dispatch directly to the captured student IDs.
        if (affectedStudentIds.Count > 0)
        {
            var studentBody =
                $"Your shuttle on {dateLabel} has been cancelled. {typeLabel}{descSuffix}: no shuttles will run that day.";
            foreach (var studentId in affectedStudentIds)
            {
                await _notificationService.DispatchAsync(
                    studentId,
                    "Trip cancelled",
                    studentBody);
            }
        }

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

    /// <summary>
    /// TripInstance statuses that can still be retroactively cancelled when a
    /// calendar exception is created. Trips already running or finished
    /// (InTransit, Completed) are left untouched — you cannot un-run a shuttle.
    /// </summary>
    private static readonly HashSet<string> CancellableTripStatuses = new()
    {
        "Scheduling",
        "Locked",
        "IncidentActive"
    };

    /// <summary>
    /// Retroactively cancels trips and their active bookings for the given date.
    /// Must be called inside an open transaction. Returns the distinct student
    /// IDs whose Confirmed bookings were cancelled, so the caller can notify them.
    /// </summary>
    private async Task<List<int>> CancelTripsForDateAsync(DateOnly targetDate)
    {
        // Trips on this date that are still in a cancellable state.
        var trips = await _context.TripInstances
            .Where(ti => ti.TripDate == targetDate &&
                         CancellableTripStatuses.Contains(ti.Status))
            .ToListAsync();

        if (trips.Count == 0)
            return new List<int>();

        var tripIds = trips.Select(ti => ti.TripInstanceId).ToList();

        // Capture Confirmed passengers BEFORE flipping statuses — these are the
        // students who had a seat and must be told the trip is off.
        var confirmedStudentIds = await _context.Bookings
            .Where(b => tripIds.Contains(b.TripInstanceId) &&
                        b.BookingStatus == "Confirmed")
            .Select(b => b.StudentId)
            .Distinct()
            .ToListAsync();

        // Cancel all active (Confirmed or WaitingList) bookings on these trips.
        var activeBookings = await _context.Bookings
            .Where(b => tripIds.Contains(b.TripInstanceId) &&
                        (b.BookingStatus == "Confirmed" ||
                         b.BookingStatus == "WaitingList"))
            .ToListAsync();

        foreach (var booking in activeBookings)
            booking.BookingStatus = "Cancelled";

        // Mark the trips themselves as Cancelled.
        foreach (var trip in trips)
            trip.Status = "Cancelled";

        await _context.SaveChangesAsync();

        return confirmedStudentIds;
    }
}
