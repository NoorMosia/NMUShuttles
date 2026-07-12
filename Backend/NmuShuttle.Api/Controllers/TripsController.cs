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
                .ThenInclude(st => st.Campus)
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

        var response = new List<TripAvailabilityDto>();

        foreach (var trip in trips)
        {
            var totalCapacity = await _capacityService.GetTotalCapacityAsync(trip.TripInstanceId);
            var confirmedCount = await _capacityService.GetConfirmedCountAsync(trip.TripInstanceId);
            var availableSeats = await _capacityService.GetAvailableSeatsAsync(trip.TripInstanceId);

            response.Add(new TripAvailabilityDto
            {
                TripInstanceId = trip.TripInstanceId,
                DepartureTime = trip.ScheduleTime.DepartureTime,
                Direction = trip.ScheduleTime.Direction,
                StopName = trip.Stop.StopName,
                CampusName = trip.ScheduleTime.Campus.CampusName,
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
                .ThenInclude(st => st.Campus)
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

        var response = new TripDetailDto
        {
            TripInstanceId = trip.TripInstanceId,
            DepartureTime = trip.ScheduleTime.DepartureTime,
            Direction = trip.ScheduleTime.Direction,
            StopName = trip.Stop.StopName,
            CampusName = trip.ScheduleTime.Campus.CampusName,
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
}
