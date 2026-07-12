using Microsoft.EntityFrameworkCore;
using NmuShuttle.Api.Data;
using NmuShuttle.Api.Infrastructure;
using NmuShuttle.Api.Models;

namespace NmuShuttle.Api.Services;

/// <summary>
/// Implements the full check-in algorithm: QR lookup, 35-min scan window,
/// marshal location binding, status validation, and automatic shuttle assignment.
/// </summary>
public class CheckInService : ICheckInService
{
    private readonly ShuttleDbContext _db;
    private static readonly TimeSpan SastOffset = TimeSpan.FromHours(2);
    private static readonly TimeSpan ScanWindowMinutes = TimeSpan.FromMinutes(35);

    public CheckInService(ShuttleDbContext db)
    {
        _db = db;
    }

    public async Task<Booking> CheckInAsync(string qrCodeToken, int marshalId)
    {
        // Step 1: Look up Booking by QrCodeToken
        var booking = await _db.Bookings
            .Include(b => b.TripInstance)
                .ThenInclude(t => t.ScheduleTime)
            .Include(b => b.TripInstance)
                .ThenInclude(t => t.Stop)
            .FirstOrDefaultAsync(b => b.QrCodeToken == qrCodeToken);

        if (booking is null)
        {
            throw new KeyNotFoundException("Booking not found for the provided QR code");
        }

        var tripInstance = booking.TripInstance;
        var scheduleTime = tripInstance.ScheduleTime;

        // Step 2: Verify 35-minute scan window
        // DepartureTime is in SAST (UTC+2). Convert to UTC: tripDate + departureTime - 2 hours
        var departureDateTime = tripInstance.TripDate.ToDateTime(scheduleTime.DepartureTime);
        var utcDeparture = departureDateTime - SastOffset;
        var scanWindowStart = utcDeparture - ScanWindowMinutes;

        if (DateTime.UtcNow < scanWindowStart)
        {
            throw new CheckInException(
                "Scanning is not yet available. Opens 35 minutes before departure.",
                StatusCodes.Status422UnprocessableEntity);
        }

        // Step 3: Verify marshal location binding
        var marshal = await _db.Marshals.FindAsync(marshalId);
        if (marshal is null)
        {
            throw new KeyNotFoundException("Marshal not found");
        }

        var direction = scheduleTime.Direction;
        if (direction == "ToCampus")
        {
            // Marshal must be assigned to the trip's stop
            if (marshal.AssignedStopId != tripInstance.StopId)
            {
                throw new CheckInException(
                    "Marshal is not authorized to scan at this location",
                    StatusCodes.Status403Forbidden);
            }
        }
        else if (direction == "FromCampus")
        {
            // Marshal must be assigned to the trip's campus
            if (marshal.AssignedCampusId != scheduleTime.CampusId)
            {
                throw new CheckInException(
                    "Marshal is not authorized to scan at this location",
                    StatusCodes.Status403Forbidden);
            }
        }

        // Step 4: Verify BookingStatus == "Confirmed"
        if (booking.BookingStatus != "Confirmed")
        {
            throw new CheckInException(
                "Booking is not in Confirmed status",
                StatusCodes.Status422UnprocessableEntity);
        }

        // Step 5: Verify BoardedStatus != "Boarded"
        if (booking.BoardedStatus == "Boarded")
        {
            throw new CheckInException(
                "Student is already boarded",
                StatusCodes.Status409Conflict);
        }

        // Step 6: Auto-assign shuttle (front-to-back by OrderPosition ASC)
        var allocations = await _db.TripShuttleAllocations
            .Where(a => a.TripInstanceId == tripInstance.TripInstanceId && a.IsOperational)
            .OrderBy(a => a.OrderPosition)
            .Include(a => a.Shuttle)
            .ToListAsync();

        int? assignedShuttleId = null;

        foreach (var allocation in allocations)
        {
            var boardedCount = await _db.Bookings
                .CountAsync(b => b.TripInstanceId == tripInstance.TripInstanceId
                    && b.ShuttleId == allocation.ShuttleId
                    && b.BoardedStatus == "Boarded");

            if (boardedCount < allocation.Shuttle.Capacity)
            {
                assignedShuttleId = allocation.ShuttleId;
                break;
            }
        }

        if (assignedShuttleId is null)
        {
            throw new CheckInException(
                "Convoy is full — all shuttles at capacity",
                StatusCodes.Status422UnprocessableEntity);
        }

        // Step 7: Update booking
        booking.BoardedStatus = "Boarded";
        booking.BoardedAt = DateTime.UtcNow;
        booking.ShuttleId = assignedShuttleId;

        await _db.SaveChangesAsync();

        return booking;
    }
}
