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
    private static readonly TimeSpan ScanWindowMinutes = TimeSpan.FromMinutes(35);

    public CheckInService(ShuttleDbContext db)
    {
        _db = db;
    }

    public async Task<Booking> CheckInAsync(string qrCodeToken, int marshalId)
    {
        // Step 1: Look up Booking by QrCodeToken
        var booking = await _db.Bookings
            .Include(b => b.Student)
            .Include(b => b.TripInstance)
                .ThenInclude(t => t.ScheduleTime)
                    .ThenInclude(st => st.ScheduleTimeCampuses)
            .Include(b => b.TripInstance)
                .ThenInclude(t => t.Stop)
            .FirstOrDefaultAsync(b => b.QrCodeToken == qrCodeToken);

        if (booking is null)
        {
            throw new KeyNotFoundException("Booking not found for the provided QR code");
        }

        var tripInstance = booking.TripInstance;
        var scheduleTime = tripInstance.ScheduleTime;

        // Step 2: RequiresScanning safety net (primary enforcement is client-side)
        if (!scheduleTime.RequiresScanning)
        {
            throw new CheckInException(
                "This trip is open boarding — no QR scan required.",
                StatusCodes.Status200OK);
        }

        // Step 3: Verify scan window. The very next upcoming trip is open at any time;
        // all other future trips only open 35 minutes before departure. A trip whose
        // departure has already passed is closed.
        await ValidateScanWindowAsync(tripInstance);

        // Step 4: Verify marshal location binding
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
            // Marshal must be assigned to one of the campuses on this route
            var routeCampusIds = scheduleTime.ScheduleTimeCampuses
                .Select(stc => stc.CampusId)
                .ToList();

            if (!routeCampusIds.Contains(marshal.AssignedCampusId ?? 0))
            {
                throw new CheckInException(
                    "Marshal is not authorized to scan at this campus for this route.",
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
        booking.BoardedAt = SastClock.UtcNow;
        booking.ShuttleId = assignedShuttleId;

        await _db.SaveChangesAsync();

        return booking;
    }

    public async Task<Booking> ManualCheckInAsync(int bookingId, int marshalId)
    {
        // Step 1: Look up Booking by ID
        var booking = await _db.Bookings
            .Include(b => b.Student)
            .Include(b => b.TripInstance)
                .ThenInclude(t => t.ScheduleTime)
                    .ThenInclude(st => st.ScheduleTimeCampuses)
            .Include(b => b.TripInstance)
                .ThenInclude(t => t.Stop)
            .FirstOrDefaultAsync(b => b.BookingId == bookingId);

        if (booking is null)
        {
            throw new KeyNotFoundException("Booking not found");
        }

        var tripInstance = booking.TripInstance;
        var scheduleTime = tripInstance.ScheduleTime;

        // Step 2: RequiresScanning safety net
        if (!scheduleTime.RequiresScanning)
        {
            throw new CheckInException(
                "This trip is open boarding — no QR scan required.",
                StatusCodes.Status200OK);
        }

        // Step 3: Verify scan window. The very next upcoming trip is open at any time;
        // all other future trips only open 35 minutes before departure. A trip whose
        // departure has already passed is closed.
        await ValidateScanWindowAsync(tripInstance);

        // Step 4: Verify marshal location binding
        var marshal = await _db.Marshals.FindAsync(marshalId);
        if (marshal is null)
        {
            throw new KeyNotFoundException("Marshal not found");
        }

        var direction = scheduleTime.Direction;
        if (direction == "ToCampus")
        {
            if (marshal.AssignedStopId != tripInstance.StopId)
            {
                throw new CheckInException(
                    "Marshal is not authorized to scan at this location",
                    StatusCodes.Status403Forbidden);
            }
        }
        else if (direction == "FromCampus")
        {
            var routeCampusIds = scheduleTime.ScheduleTimeCampuses
                .Select(stc => stc.CampusId)
                .ToList();

            if (!routeCampusIds.Contains(marshal.AssignedCampusId ?? 0))
            {
                throw new CheckInException(
                    "Marshal is not authorized to scan at this campus for this route.",
                    StatusCodes.Status403Forbidden);
            }
        }

        // Step 5: Verify BookingStatus == "Confirmed"
        if (booking.BookingStatus != "Confirmed")
        {
            throw new CheckInException(
                "Booking is not in Confirmed status",
                StatusCodes.Status422UnprocessableEntity);
        }

        // Step 6: Verify BoardedStatus != "Boarded"
        if (booking.BoardedStatus == "Boarded")
        {
            throw new CheckInException(
                "Student is already boarded",
                StatusCodes.Status409Conflict);
        }

        // Step 7: Auto-assign shuttle (front-to-back by OrderPosition ASC)
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

        // Step 8: Update booking
        booking.BoardedStatus = "Boarded";
        booking.BoardedAt = SastClock.UtcNow;
        booking.ShuttleId = assignedShuttleId;

        await _db.SaveChangesAsync();

        return booking;
    }

    /// <summary>
    /// Converts a trip's SAST date + departure time into its UTC departure instant.
    /// </summary>
    private static DateTime ToUtcDeparture(DateOnly tripDate, TimeOnly departureTime)
        => SastClock.ToUtc(tripDate, departureTime);

    /// <summary>
    /// Determines whether the given trip is the "very next" upcoming trip overall —
    /// the trip with the earliest departure instant that has not yet departed, across
    /// the entire system (all stops, campuses, and directions). The next trip is open
    /// to scanning at any time; every other future trip only opens 35 minutes before
    /// its own departure.
    /// </summary>
    private async Task<bool> IsNextUpcomingTripAsync(TripInstance tripInstance)
    {
        var now = SastClock.UtcNow;

        // Narrow the candidate set to today's and tomorrow's trips (a departure can only
        // be "next" if it is near-term), then compute UTC departure instants in memory
        // because TripDate (DateOnly) and DepartureTime (TimeOnly) are stored separately.
        var today = SastClock.Today;
        var tomorrow = today.AddDays(1);

        var candidates = await _db.TripInstances
            .Where(t => t.ScheduleTime.RequiresScanning
                && (t.TripDate == today || t.TripDate == tomorrow))
            .Select(t => new
            {
                t.TripInstanceId,
                t.TripDate,
                t.ScheduleTime.DepartureTime
            })
            .ToListAsync();

        // The next trip is the one with the smallest departure instant still in the future.
        var next = candidates
            .Select(c => new
            {
                c.TripInstanceId,
                Departure = ToUtcDeparture(c.TripDate, c.DepartureTime)
            })
            .Where(c => c.Departure >= now)
            .OrderBy(c => c.Departure)
            .FirstOrDefault();

        return next is not null && next.TripInstanceId == tripInstance.TripInstanceId;
    }

    /// <summary>
    /// Enforces the scan window. The very next upcoming trip is open at any time until
    /// its departure passes. Any other future trip only opens 35 minutes before departure.
    /// Throws <see cref="CheckInException"/> (422) when scanning is not permitted.
    /// </summary>
    private async Task ValidateScanWindowAsync(TripInstance tripInstance)
    {
        var now = SastClock.UtcNow;
        var utcDeparture = ToUtcDeparture(tripInstance.TripDate, tripInstance.ScheduleTime.DepartureTime);

        // Once a trip's departure has passed it is no longer upcoming — scanning is closed.
        if (now > utcDeparture)
        {
            throw new CheckInException(
                "Scanning is closed — this trip has already departed.",
                StatusCodes.Status422UnprocessableEntity);
        }

        // The very next trip is open at any point. All other future trips are gated to
        // the 35-minute pre-departure window.
        var isNextTrip = await IsNextUpcomingTripAsync(tripInstance);
        if (!isNextTrip)
        {
            var scanWindowStart = utcDeparture - ScanWindowMinutes;
            if (now < scanWindowStart)
            {
                throw new CheckInException(
                    "Scanning is not yet available. Opens 35 minutes before departure.",
                    StatusCodes.Status422UnprocessableEntity);
            }
        }
    }
}
