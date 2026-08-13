using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NmuShuttle.Api.Data;
using NmuShuttle.Api.DTOs;
using NmuShuttle.Api.Services;

namespace NmuShuttle.Api.Controllers;

/// <summary>
/// Handles QR code check-in operations performed by Marshals at boarding points.
/// </summary>
[ApiController]
[Route("api/v1/checkin")]
[Authorize(Roles = "Marshal")]
public class CheckInController : ControllerBase
{
    private readonly ICheckInService _checkInService;
    private readonly ShuttleDbContext _dbContext;

    public CheckInController(ICheckInService checkInService, ShuttleDbContext dbContext)
    {
        _checkInService = checkInService;
        _dbContext = dbContext;
    }

    /// <summary>
    /// Scans a student's QR code to check them in for their booked trip.
    /// Extracts the marshal's ID from the JWT sub claim and delegates to CheckInService.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CheckIn([FromBody] CheckInRequestDto dto)
    {
        var marshalIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                             ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(marshalIdClaim) || !int.TryParse(marshalIdClaim, out var marshalId))
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = "Invalid or missing marshal identity claim.",
                Instance = HttpContext.Request.Path
            });
        }

        // Delegates to CheckInService; exceptions handled by GlobalExceptionHandler
        var booking = await _checkInService.CheckInAsync(dto.QrCodeToken, marshalId);

        // Fetch shuttle details and convoy position for the response
        string? licensePlate = null;
        int? convoyPosition = null;
        int? shuttleCapacity = null;

        if (booking.ShuttleId.HasValue)
        {
            var allocation = await _dbContext.TripShuttleAllocations
                .Include(a => a.Shuttle)
                .FirstOrDefaultAsync(a =>
                    a.TripInstanceId == booking.TripInstanceId
                    && a.ShuttleId == booking.ShuttleId.Value);

            if (allocation != null)
            {
                licensePlate = allocation.Shuttle.LicensePlate;
                convoyPosition = allocation.OrderPosition;
                shuttleCapacity = allocation.Shuttle.Capacity;
            }
        }

        return Ok(new
        {
            booking.BookingId,
            booking.Student.FirstName,
            booking.Student.LastName,
            StudentName = $"{booking.Student.FirstName} {booking.Student.LastName}",
            booking.Student.StudentNumber,
            StopName = booking.TripInstance.Stop.StopName,
            booking.BoardedStatus,
            booking.ShuttleId,
            LicensePlate = licensePlate,
            ConvoyPosition = convoyPosition,
            ShuttleCapacity = shuttleCapacity,
            booking.BoardedAt
        });
    }

    /// <summary>
    /// Manual check-in by booking ID. Used when a student's phone is dead
    /// and the marshal finds them in the passenger list.
    /// </summary>
    [HttpPost("manual")]
    public async Task<IActionResult> ManualCheckIn([FromBody] ManualCheckInRequestDto dto)
    {
        var marshalIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                             ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(marshalIdClaim) || !int.TryParse(marshalIdClaim, out var marshalId))
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = "Invalid or missing marshal identity claim.",
                Instance = HttpContext.Request.Path
            });
        }

        var booking = await _checkInService.ManualCheckInAsync(dto.BookingId, marshalId);

        // Fetch shuttle details and convoy position
        string? licensePlate = null;
        int? convoyPosition = null;
        int? shuttleCapacity = null;

        if (booking.ShuttleId.HasValue)
        {
            var allocation = await _dbContext.TripShuttleAllocations
                .Include(a => a.Shuttle)
                .FirstOrDefaultAsync(a =>
                    a.TripInstanceId == booking.TripInstanceId
                    && a.ShuttleId == booking.ShuttleId.Value);

            if (allocation != null)
            {
                licensePlate = allocation.Shuttle.LicensePlate;
                convoyPosition = allocation.OrderPosition;
                shuttleCapacity = allocation.Shuttle.Capacity;
            }
        }

        return Ok(new
        {
            booking.BookingId,
            booking.Student.FirstName,
            booking.Student.LastName,
            StudentName = $"{booking.Student.FirstName} {booking.Student.LastName}",
            booking.Student.StudentNumber,
            StopName = booking.TripInstance.Stop.StopName,
            booking.BoardedStatus,
            booking.ShuttleId,
            LicensePlate = licensePlate,
            ConvoyPosition = convoyPosition,
            ShuttleCapacity = shuttleCapacity,
            booking.BoardedAt
        });
    }

    /// <summary>
    /// Returns the passenger manifest for a specific trip, including boarding counts.
    /// Accessible by Marshals and Admins.
    /// </summary>
    [HttpGet("/api/v1/trips/{tripId:int}/passengers")]
    [Authorize(Roles = "Marshal,Admin")]
    public async Task<IActionResult> GetPassengerManifest(int tripId)
    {
        // Verify trip exists
        var tripExists = await _dbContext.TripInstances.AnyAsync(t => t.TripInstanceId == tripId);
        if (!tripExists)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found",
                Detail = $"Trip instance with ID {tripId} not found.",
                Instance = HttpContext.Request.Path
            });
        }

        var bookings = await _dbContext.Bookings
            .Where(b => b.TripInstanceId == tripId && b.BookingStatus == "Confirmed")
            .Include(b => b.Student)
            .OrderBy(b => b.CreatedAt)
            .ToListAsync();

        var totalConfirmed = bookings.Count;
        var boardedCount = bookings.Count(b => b.BoardedStatus == "Boarded");
        var pendingCount = bookings.Count(b => b.BoardedStatus == "Pending");

        var manifest = new PassengerManifestDto
        {
            TripInstanceId = tripId,
            TotalConfirmed = totalConfirmed,
            BoardedCount = boardedCount,
            PendingCount = pendingCount,
            Passengers = bookings.Select(b => new PassengerDto
            {
                BookingId = b.BookingId,
                StudentName = $"{b.Student.FirstName} {b.Student.LastName}",
                StudentNumber = b.Student.StudentNumber,
                BoardedStatus = b.BoardedStatus,
                ShuttleId = b.ShuttleId,
                BoardedAt = b.BoardedAt
            }).ToList()
        };

        return Ok(manifest);
    }
}
