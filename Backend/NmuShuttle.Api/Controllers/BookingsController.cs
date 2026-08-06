using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NmuShuttle.Api.DTOs;
using NmuShuttle.Api.Services;

namespace NmuShuttle.Api.Controllers;

/// <summary>
/// Student-facing booking endpoints for creating, viewing, and cancelling shuttle bookings.
/// </summary>
[ApiController]
[Route("api/v1/bookings")]
[Authorize(Roles = "Student")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    /// <summary>
    /// Create a new booking for the authenticated student on a trip instance.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(BookingResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto dto)
    {
        var studentId = GetStudentId();

        try
        {
            var booking = await _bookingService.CreateBookingAsync(studentId, dto.TripInstanceId);

            // Reload with navigation properties for response DTO
            var fullBooking = await _bookingService.GetBookingByIdAsync(booking.BookingId, studentId);
            if (fullBooking == null)
                return StatusCode(500, new ProblemDetails { Detail = "Booking created but could not be retrieved" });

            var response = new BookingResponseDto
            {
                BookingId = fullBooking.BookingId,
                BookingStatus = fullBooking.BookingStatus,
                BookingType = fullBooking.BookingType,
                QrCodeToken = fullBooking.QrCodeToken,
                TripDate = fullBooking.TripInstance.TripDate,
                DepartureTime = fullBooking.TripInstance.ScheduleTime.DepartureTime,
                Direction = fullBooking.TripInstance.ScheduleTime.Direction,
                StopName = fullBooking.TripInstance.Stop.StopName,
                CampusName = fullBooking.TripInstance.ScheduleTime.ScheduleTimeCampuses
                    .OrderBy(stc => stc.StopOrder).FirstOrDefault()?.Campus?.CampusName ?? "",
                BoardedStatus = fullBooking.BoardedStatus,
                CreatedAt = fullBooking.CreatedAt
            };

            return CreatedAtAction(nameof(GetBooking), new { id = fullBooking.BookingId }, response);
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
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad Request",
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
    /// Get all bookings for the authenticated student.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<BookingResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBookings()
    {
        var studentId = GetStudentId();
        var bookings = await _bookingService.GetStudentBookingsAsync(studentId);

        var response = bookings.Select(b => new BookingResponseDto
        {
            BookingId = b.BookingId,
            BookingStatus = b.BookingStatus,
            BookingType = b.BookingType,
            QrCodeToken = b.QrCodeToken,
            TripDate = b.TripInstance.TripDate,
            DepartureTime = b.TripInstance.ScheduleTime.DepartureTime,
            Direction = b.TripInstance.ScheduleTime.Direction,
            StopName = b.TripInstance.Stop.StopName,
            CampusName = b.TripInstance.ScheduleTime.ScheduleTimeCampuses
                .OrderBy(stc => stc.StopOrder).FirstOrDefault()?.Campus?.CampusName ?? "",
            BoardedStatus = b.BoardedStatus,
            CreatedAt = b.CreatedAt
        }).ToList();

        return Ok(response);
    }

    /// <summary>
    /// Get a specific booking by ID for the authenticated student.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(BookingResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBooking(int id)
    {
        var studentId = GetStudentId();
        var booking = await _bookingService.GetBookingByIdAsync(id, studentId);

        if (booking is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found",
                Detail = "Booking not found.",
                Instance = HttpContext.Request.Path
            });
        }

        var response = new BookingResponseDto
        {
            BookingId = booking.BookingId,
            BookingStatus = booking.BookingStatus,
            BookingType = booking.BookingType,
            QrCodeToken = booking.QrCodeToken,
            TripDate = booking.TripInstance.TripDate,
            DepartureTime = booking.TripInstance.ScheduleTime.DepartureTime,
            Direction = booking.TripInstance.ScheduleTime.Direction,
            StopName = booking.TripInstance.Stop.StopName,
            CampusName = booking.TripInstance.ScheduleTime.ScheduleTimeCampuses
                .OrderBy(stc => stc.StopOrder).FirstOrDefault()?.Campus?.CampusName ?? "",
            BoardedStatus = booking.BoardedStatus,
            CreatedAt = booking.CreatedAt
        };

        return Ok(response);
    }

    /// <summary>
    /// Cancel a booking. Only allowed while the trip is in Scheduling status.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelBooking(int id)
    {
        var studentId = GetStudentId();

        try
        {
            var booking = await _bookingService.CancelBookingAsync(id, studentId);

            return Ok(new { Message = "Booking cancelled successfully.", BookingId = booking.BookingId });
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
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad Request",
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

    private int GetStudentId()
    {
        var subClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                       ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.Parse(subClaim!);
    }
}
