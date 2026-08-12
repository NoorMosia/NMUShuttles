using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NmuShuttle.Api.Data;
using NmuShuttle.Api.DTOs;
using NmuShuttle.Api.Services;

namespace NmuShuttle.Api.Controllers;

/// <summary>
/// Trip availability endpoints accessible to all authenticated roles (Student, Marshal, Admin).
/// </summary>
[ApiController]
[Route("api/v1/trips")]
[Authorize]
public class TripsController : ControllerBase
{
    private readonly ShuttleDbContext _dbContext;
    private readonly ICapacityService _capacityService;

    public TripsController(ShuttleDbContext dbContext, ICapacityService capacityService)
    {
        _dbContext = dbContext;
        _capacityService = capacityService;
    }

    /// <summary>
    /// Get available trips filtered by date, stop, and/or direction.
    /// Returns capacity information for each trip.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<TripAvailabilityDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTrips(
        [FromQuery] DateOnly? date,
        [FromQuery] int? stopId,
        [FromQuery] string? direction)
    {
        var query = _dbContext.TripInstances
            .Include(t => t.ScheduleTime)
                .ThenInclude(st => st.ScheduleTimeCampuses)
                    .ThenInclude(stc => stc.Campus)
            .Include(t => t.Stop)
            .AsQueryable();

        if (date.HasValue)
        {
            query = query.Where(t => t.TripDate == date.Value);
        }

        if (stopId.HasValue)
        {
            query = query.Where(t => t.StopId == stopId.Value);
        }

        if (!string.IsNullOrWhiteSpace(direction))
        {
            query = query.Where(t => t.ScheduleTime.Direction == direction);
        }

        var trips = await query
            .OrderBy(t => t.TripDate)
            .ThenBy(t => t.ScheduleTime.DepartureTime)
            .ToListAsync();

        if (trips.Count == 0)
            return Ok(new List<TripAvailabilityDto>());

        // Batch capacity calculation — avoids N+1 queries
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

        var response = new List<TripAvailabilityDto>();

        foreach (var trip in trips)
        {
            var totalCapacity = capacityByTrip.GetValueOrDefault(trip.TripInstanceId, 0);
            var confirmedCount = confirmedByTrip.GetValueOrDefault(trip.TripInstanceId, 0);
            var availableSeats = Math.Max(0, totalCapacity - confirmedCount);

            // Backward compat: return first campus (StopOrder=1) as campusName
            var firstCampus = trip.ScheduleTime.ScheduleTimeCampuses
                .OrderBy(stc => stc.StopOrder)
                .FirstOrDefault();

            response.Add(new TripAvailabilityDto
            {
                TripInstanceId = trip.TripInstanceId,
                DepartureTime = trip.ScheduleTime.DepartureTime,
                Direction = trip.ScheduleTime.Direction,
                StopName = trip.Stop.StopName,
                CampusName = firstCampus?.Campus?.CampusName ?? "",
                TripDate = trip.TripDate,
                Status = trip.Status,
                TotalCapacity = totalCapacity,
                ConfirmedCount = confirmedCount,
                AvailableSeats = availableSeats
            });
        }

        return Ok(response);
    }

    /// <summary>
    /// Get a single trip detail with allocations and capacity info.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TripDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTrip(int id)
    {
        var trip = await _dbContext.TripInstances
            .Include(t => t.ScheduleTime)
                .ThenInclude(st => st.ScheduleTimeCampuses)
                    .ThenInclude(stc => stc.Campus)
            .Include(t => t.Stop)
            .Include(t => t.TripShuttleAllocations)
                .ThenInclude(a => a.Shuttle)
            .FirstOrDefaultAsync(t => t.TripInstanceId == id);

        if (trip is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found",
                Detail = "Trip not found.",
                Instance = HttpContext.Request.Path
            });
        }

        var totalCapacity = await _capacityService.GetTotalCapacityAsync(trip.TripInstanceId);
        var confirmedCount = await _capacityService.GetConfirmedCountAsync(trip.TripInstanceId);
        var availableSeats = await _capacityService.GetAvailableSeatsAsync(trip.TripInstanceId);

        // Backward compat: return first campus (StopOrder=1) as campusName
        var firstCampus = trip.ScheduleTime.ScheduleTimeCampuses
            .OrderBy(stc => stc.StopOrder)
            .FirstOrDefault();

        var response = new TripDetailDto
        {
            TripInstanceId = trip.TripInstanceId,
            DepartureTime = trip.ScheduleTime.DepartureTime,
            Direction = trip.ScheduleTime.Direction,
            StopName = trip.Stop.StopName,
            CampusName = firstCampus?.Campus?.CampusName ?? "",
            TripDate = trip.TripDate,
            Status = trip.Status,
            TotalCapacity = totalCapacity,
            ConfirmedCount = confirmedCount,
            AvailableSeats = availableSeats,
            LockedAt = trip.LockedAt,
            InTransitAt = trip.InTransitAt,
            CompletedAt = trip.CompletedAt,
            Allocations = trip.TripShuttleAllocations
                .OrderBy(a => a.OrderPosition)
                .Select(a => new TripAllocationDto
                {
                    AllocationId = a.AllocationId,
                    ShuttleId = a.ShuttleId,
                    LicensePlate = a.Shuttle.LicensePlate,
                    Capacity = a.Shuttle.Capacity,
                    IsAccessibleVehicle = a.Shuttle.IsAccessibleVehicle,
                    OrderPosition = a.OrderPosition,
                    IsOperational = a.IsOperational
                }).ToList()
        };

        return Ok(response);
    }

    /// <summary>
    /// Get convoy status for a trip — shuttle allocations with live boarded counts.
    /// Accessible to Marshal and Admin roles.
    /// </summary>
    [HttpGet("{tripId}/convoy")]
    [Authorize(Roles = "Marshal,Admin")]
    public async Task<IActionResult> GetConvoyStatus(int tripId)
    {
        var trip = await _dbContext.TripInstances
            .Include(t => t.ScheduleTime)
            .FirstOrDefaultAsync(t => t.TripInstanceId == tripId);

        if (trip is null)
            return NotFound(new { message = "Trip not found" });

        var allocations = await _dbContext.TripShuttleAllocations
            .Include(a => a.Shuttle)
            .Where(a => a.TripInstanceId == tripId)
            .OrderBy(a => a.OrderPosition)
            .ToListAsync();

        var shuttles = new List<object>();

        foreach (var allocation in allocations)
        {
            var boardedCount = await _dbContext.Bookings
                .CountAsync(b =>
                    b.TripInstanceId == tripId &&
                    b.ShuttleId == allocation.ShuttleId &&
                    b.BoardedStatus == "Boarded");

            shuttles.Add(new
            {
                allocationId = allocation.AllocationId,
                orderPosition = allocation.OrderPosition,
                shuttleId = allocation.ShuttleId,
                licensePlate = allocation.Shuttle.LicensePlate,
                isAccessibleVehicle = allocation.Shuttle.IsAccessibleVehicle,
                capacity = allocation.Shuttle.Capacity,
                boardedCount,
                isOperational = allocation.IsOperational
            });
        }

        return Ok(new
        {
            tripInstanceId = tripId,
            requiresScanning = trip.ScheduleTime.RequiresScanning,
            shuttles
        });
    }
}
