using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NmuShuttle.Api.Data;
using NmuShuttle.Api.DTOs;

namespace NmuShuttle.Api.Controllers;

/// <summary>
/// Marshal-specific trip endpoints that return grouped time-slot views.
/// Campus marshals see trips aggregated by departure time across all stops.
/// Stop marshals see their stop's trips only.
/// </summary>
[ApiController]
[Route("api/v1/marshal/trips")]
[Authorize(Roles = "Marshal")]
public class MarshalTripsController : ControllerBase
{
    private readonly ShuttleDbContext _dbContext;
    private static readonly TimeSpan SastOffset = TimeSpan.FromHours(2);
    private static readonly TimeSpan ScanWindowMinutes = TimeSpan.FromMinutes(35);

    public MarshalTripsController(ShuttleDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Get today's trips for the authenticated marshal, grouped by time slot.
    /// Campus marshals get all trips (both directions) for their campus, aggregated by departure time.
    /// Stop marshals get their stop's trips (both directions) as individual time slots.
    /// </summary>
    [HttpGet("today")]
    [ProducesResponseType(typeof(List<MarshalTimeSlotDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTodayTimeSlots()
    {
        var marshalId = GetMarshalId();

        var marshal = await _dbContext.Marshals
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.MarshalId == marshalId);

        if (marshal is null)
            return NotFound(new { message = "Marshal not found" });

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Add(SastOffset));

        var query = _dbContext.TripInstances
            .Include(t => t.ScheduleTime)
                .ThenInclude(st => st.ScheduleTimeCampuses)
            .Include(t => t.Stop)
            .Where(t => t.TripDate == today);

        if (marshal.AssignedCampusId.HasValue && marshal.AssignedStopId.HasValue)
        {
            // Both assigned: ToCampus for their stop + FromCampus for their campus
            query = query.Where(t =>
                (t.ScheduleTime.Direction == "ToCampus" && t.StopId == marshal.AssignedStopId.Value)
                || (t.ScheduleTime.Direction == "FromCampus"
                    && t.ScheduleTime.ScheduleTimeCampuses.Any(stc => stc.CampusId == marshal.AssignedCampusId.Value)));
        }
        else if (marshal.AssignedCampusId.HasValue)
        {
            // Campus marshal: FromCampus trips where their campus is on the route
            query = query.Where(t =>
                t.ScheduleTime.Direction == "FromCampus"
                && t.ScheduleTime.ScheduleTimeCampuses.Any(stc => stc.CampusId == marshal.AssignedCampusId.Value));
        }
        else if (marshal.AssignedStopId.HasValue)
        {
            // Stop marshal: ToCampus trips for their stop
            query = query.Where(t =>
                t.ScheduleTime.Direction == "ToCampus"
                && t.StopId == marshal.AssignedStopId.Value);
        }
        else
        {
            return Ok(new List<MarshalTimeSlotDto>());
        }

        var trips = await query
            .OrderBy(t => t.ScheduleTime.DepartureTime)
            .ToListAsync();

        if (trips.Count == 0)
            return Ok(new List<MarshalTimeSlotDto>());

        // Batch capacity + boarded counts
        var tripIds = trips.Select(t => t.TripInstanceId).ToList();

        var capacityByTrip = await _dbContext.TripShuttleAllocations
            .Where(a => tripIds.Contains(a.TripInstanceId) && a.IsOperational)
            .Join(
                _dbContext.Shuttles,
                a => a.ShuttleId,
                s => s.ShuttleId,
                (a, s) => new { a.TripInstanceId, s.Capacity })
            .GroupBy(x => x.TripInstanceId)
            .Select(g => new { TripInstanceId = g.Key, TotalCapacity = g.Sum(x => x.Capacity) })
            .ToDictionaryAsync(x => x.TripInstanceId, x => x.TotalCapacity);

        var confirmedByTrip = await _dbContext.Bookings
            .Where(b => tripIds.Contains(b.TripInstanceId) && b.BookingStatus == "Confirmed")
            .GroupBy(b => b.TripInstanceId)
            .Select(g => new { TripInstanceId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TripInstanceId, x => x.Count);

        var boardedByTrip = await _dbContext.Bookings
            .Where(b => tripIds.Contains(b.TripInstanceId) && b.BoardedStatus == "Boarded")
            .GroupBy(b => b.TripInstanceId)
            .Select(g => new { TripInstanceId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TripInstanceId, x => x.Count);

        // Determine the "very next" upcoming trip overall (system-wide, all stops/
        // campuses/directions). That trip is open to scanning at any time; every other
        // future trip only opens 35 minutes before departure. This mirrors the authority
        // in CheckInService.ValidateScanWindowAsync.
        var nowUtc = DateTime.UtcNow;
        var sastToday = DateOnly.FromDateTime(nowUtc.Add(SastOffset));
        var sastTomorrow = sastToday.AddDays(1);

        var upcomingCandidates = await _dbContext.TripInstances
            .Where(t => t.ScheduleTime.RequiresScanning
                && (t.TripDate == sastToday || t.TripDate == sastTomorrow))
            .Select(t => new { t.TripDate, t.ScheduleTime.DepartureTime })
            .ToListAsync();

        DateTime? nextDeparture = upcomingCandidates
            .Select(c => c.TripDate.ToDateTime(c.DepartureTime) - SastOffset)
            .Where(d => d >= nowUtc)
            .OrderBy(d => d)
            .Cast<DateTime?>()
            .FirstOrDefault();

        // Group trips by (DepartureTime, Direction) for campus marshals
        var grouped = trips
            .GroupBy(t => new { t.ScheduleTime.DepartureTime, t.ScheduleTime.Direction })
            .OrderBy(g => g.Key.DepartureTime)
            .ThenBy(g => g.Key.Direction)
            .Select(g =>
            {
                var groupTrips = g.ToList();
                var groupIds = groupTrips.Select(t => t.TripInstanceId).ToList();

                var totalCapacity = groupIds.Sum(id => capacityByTrip.GetValueOrDefault(id, 0));
                var confirmedCount = groupIds.Sum(id => confirmedByTrip.GetValueOrDefault(id, 0));
                var boardedCount = groupIds.Sum(id => boardedByTrip.GetValueOrDefault(id, 0));

                // Determine aggregate status (most active wins)
                var status = ResolveGroupStatus(groupTrips.Select(t => t.Status));

                // Check if scanning is open.
                var departureDateTime = today.ToDateTime(g.Key.DepartureTime);
                var utcDeparture = departureDateTime - SastOffset;
                var scanWindowStart = utcDeparture - ScanWindowMinutes;
                var now = DateTime.UtcNow;

                // The very next upcoming trip overall is open at any time until it departs.
                // Every other future trip is gated to the 35-minute pre-departure window.
                var isNextTrip = nextDeparture.HasValue && utcDeparture == nextDeparture.Value;
                var isScanningOpen = now <= utcDeparture
                    && (isNextTrip || now >= scanWindowStart);

                return new MarshalTimeSlotDto
                {
                    DepartureTime = g.Key.DepartureTime,
                    Direction = g.Key.Direction,
                    TripDate = today,
                    Status = status,
                    TotalCapacity = totalCapacity,
                    ConfirmedCount = confirmedCount,
                    AvailableSeats = Math.Max(0, totalCapacity - confirmedCount),
                    BoardedCount = boardedCount,
                    StopCount = groupTrips.Select(t => t.StopId).Distinct().Count(),
                    IsScanningOpen = isScanningOpen,
                    TripInstanceIds = groupIds
                };
            })
            .ToList();

        return Ok(grouped);
    }

    /// <summary>
    /// Get the passenger list for a time slot (all confirmed bookings across all trips in the slot).
    /// Supports optional search by student name or number.
    /// </summary>
    [HttpGet("timeslot/passengers")]
    [ProducesResponseType(typeof(MarshalPassengerListDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTimeSlotPassengers(
        [FromQuery] DateOnly date,
        [FromQuery] string departureTime,
        [FromQuery] string direction,
        [FromQuery] string? search)
    {
        var marshalId = GetMarshalId();
        var marshal = await _dbContext.Marshals.AsNoTracking()
            .FirstOrDefaultAsync(m => m.MarshalId == marshalId);

        if (marshal is null)
            return NotFound(new { message = "Marshal not found" });

        if (!TimeOnly.TryParse(departureTime, out var parsedTime))
            return BadRequest(new { message = "Invalid departureTime format. Use HH:mm" });

        // Build query for trips in this time slot
        var tripsQuery = _dbContext.TripInstances
            .Include(t => t.ScheduleTime)
                .ThenInclude(st => st.ScheduleTimeCampuses)
            .Include(t => t.Stop)
            .Where(t => t.TripDate == date
                && t.ScheduleTime.DepartureTime == parsedTime
                && t.ScheduleTime.Direction == direction);

        // Apply location filter
        if (marshal.AssignedCampusId.HasValue)
        {
            tripsQuery = tripsQuery.Where(t =>
                t.ScheduleTime.ScheduleTimeCampuses.Any(stc => stc.CampusId == marshal.AssignedCampusId.Value));
        }
        else if (marshal.AssignedStopId.HasValue)
        {
            tripsQuery = tripsQuery.Where(t => t.StopId == marshal.AssignedStopId.Value);
        }

        var tripIds = await tripsQuery
            .Select(t => t.TripInstanceId)
            .ToListAsync();

        if (tripIds.Count == 0)
            return Ok(new MarshalPassengerListDto());

        // Get all confirmed bookings for these trips
        var bookingsQuery = _dbContext.Bookings
            .Where(b => tripIds.Contains(b.TripInstanceId) && b.BookingStatus == "Confirmed")
            .Include(b => b.Student)
            .Include(b => b.TripInstance)
                .ThenInclude(t => t.Stop)
            .AsQueryable();

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            bookingsQuery = bookingsQuery.Where(b =>
                b.Student.FirstName.ToLower().Contains(searchLower)
                || b.Student.LastName.ToLower().Contains(searchLower)
                || b.Student.StudentNumber.ToLower().Contains(searchLower));
        }

        var bookings = await bookingsQuery
            .OrderBy(b => b.Student.LastName)
            .ThenBy(b => b.Student.FirstName)
            .ToListAsync();

        var result = new MarshalPassengerListDto
        {
            DepartureTime = parsedTime,
            Direction = direction,
            TripDate = date,
            TotalConfirmed = bookings.Count,
            BoardedCount = bookings.Count(b => b.BoardedStatus == "Boarded"),
            PendingCount = bookings.Count(b => b.BoardedStatus == "Pending"),
            Passengers = bookings.Select(b => new MarshalPassengerDto
            {
                BookingId = b.BookingId,
                StudentName = $"{b.Student.FirstName} {b.Student.LastName}",
                StudentNumber = b.Student.StudentNumber,
                StopName = b.TripInstance.Stop.StopName,
                BoardedStatus = b.BoardedStatus,
                ShuttleId = b.ShuttleId,
                BoardedAt = b.BoardedAt
            }).ToList()
        };

        return Ok(result);
    }

    /// <summary>
    /// Resolves the most active status from a group of trip statuses.
    /// Priority: Incident Active > In Transit > Locked > Scheduling > Completed.
    /// </summary>
    private static string ResolveGroupStatus(IEnumerable<string> statuses)
    {
        var statusList = statuses.ToList();

        if (statusList.Any(s => s == "Incident Active")) return "Incident Active";
        if (statusList.Any(s => s == "In Transit")) return "In Transit";
        if (statusList.Any(s => s == "Locked")) return "Locked";
        if (statusList.Any(s => s == "Scheduling")) return "Scheduling";
        return "Completed";
    }

    private int GetMarshalId()
    {
        var subClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                       ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.Parse(subClaim!);
    }
}
