using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NmuShuttle.Api.Data;
using NmuShuttle.Api.DTOs.Admin;
using NmuShuttle.Api.Models;
using NmuShuttle.Api.Services;

namespace NmuShuttle.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/stops")]
[Authorize(Roles = "Admin")]
public class StopsAdminController : ControllerBase
{
    private readonly ShuttleDbContext _context;
    private readonly IStopDefaultConvoyService _defaultConvoyService;

    public StopsAdminController(ShuttleDbContext context, IStopDefaultConvoyService defaultConvoyService)
    {
        _context = context;
        _defaultConvoyService = defaultConvoyService;
    }

    /// <summary>
    /// List all stops.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var stops = await _context.Stops
            .Select(s => new StopResponseDto
            {
                StopId = s.StopId,
                StopName = s.StopName,
                Street = s.Street,
                Suburb = s.Suburb,
                City = s.City,
                PostalCode = s.PostalCode,
                FullAddress = s.Street + ", " + s.Suburb + ", " + s.City + ", " + s.PostalCode,
                GpsCoordinates = s.GpsCoordinates,
                IsActive = s.IsActive
            })
            .ToListAsync();

        return Ok(stops);
    }

    /// <summary>
    /// Get a single stop by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var stop = await _context.Stops.FindAsync(id);
        if (stop == null)
            return NotFound(new { message = "Stop not found" });

        return Ok(new StopResponseDto
        {
            StopId = stop.StopId,
            StopName = stop.StopName,
            Street = stop.Street,
            Suburb = stop.Suburb,
            City = stop.City,
            PostalCode = stop.PostalCode,
            FullAddress = stop.FullAddress,
            GpsCoordinates = stop.GpsCoordinates,
            IsActive = stop.IsActive
        });
    }

    /// <summary>
    /// Create a new stop. Validates required fields and GPS format;
    /// rejects duplicate StopName with 409.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStopDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.StopName))
            return UnprocessableEntity(new { message = "StopName is required" });
        if (string.IsNullOrWhiteSpace(dto.Street))
            return UnprocessableEntity(new { message = "Street is required" });
        if (string.IsNullOrWhiteSpace(dto.Suburb))
            return UnprocessableEntity(new { message = "Suburb is required" });
        if (string.IsNullOrWhiteSpace(dto.City))
            return UnprocessableEntity(new { message = "City is required" });
        if (string.IsNullOrWhiteSpace(dto.PostalCode))
            return UnprocessableEntity(new { message = "PostalCode is required" });
        if (string.IsNullOrWhiteSpace(dto.GpsCoordinates))
            return UnprocessableEntity(new { message = "GpsCoordinates is required" });

        if (!TryParseGpsCoordinates(dto.GpsCoordinates, out _, out _))
            return UnprocessableEntity(new { message = "Invalid GPS coordinates. Expected format: 'lat,lon' with lat in [-90,90] and lon in [-180,180]" });

        var duplicate = await _context.Stops
            .AnyAsync(s => s.StopName.ToLower() == dto.StopName.ToLower());
        if (duplicate)
            return Conflict(new { message = "A stop with this name already exists" });

        var stop = new Stop
        {
            StopName = dto.StopName,
            Street = dto.Street,
            Suburb = dto.Suburb,
            City = dto.City,
            PostalCode = dto.PostalCode,
            GpsCoordinates = dto.GpsCoordinates,
            IsActive = dto.IsActive
        };

        _context.Stops.Add(stop);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = stop.StopId }, new StopResponseDto
        {
            StopId = stop.StopId,
            StopName = stop.StopName,
            Street = stop.Street,
            Suburb = stop.Suburb,
            City = stop.City,
            PostalCode = stop.PostalCode,
            FullAddress = stop.FullAddress,
            GpsCoordinates = stop.GpsCoordinates,
            IsActive = stop.IsActive
        });
    }

    /// <summary>
    /// Update an existing stop.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateStopDto dto)
    {
        var stop = await _context.Stops.FindAsync(id);
        if (stop == null)
            return NotFound(new { message = "Stop not found" });

        if (string.IsNullOrWhiteSpace(dto.StopName))
            return UnprocessableEntity(new { message = "StopName is required" });
        if (string.IsNullOrWhiteSpace(dto.Street))
            return UnprocessableEntity(new { message = "Street is required" });
        if (string.IsNullOrWhiteSpace(dto.Suburb))
            return UnprocessableEntity(new { message = "Suburb is required" });
        if (string.IsNullOrWhiteSpace(dto.City))
            return UnprocessableEntity(new { message = "City is required" });
        if (string.IsNullOrWhiteSpace(dto.PostalCode))
            return UnprocessableEntity(new { message = "PostalCode is required" });
        if (string.IsNullOrWhiteSpace(dto.GpsCoordinates))
            return UnprocessableEntity(new { message = "GpsCoordinates is required" });

        if (!TryParseGpsCoordinates(dto.GpsCoordinates, out _, out _))
            return UnprocessableEntity(new { message = "Invalid GPS coordinates. Expected format: 'lat,lon' with lat in [-90,90] and lon in [-180,180]" });

        var duplicate = await _context.Stops
            .AnyAsync(s => s.StopName.ToLower() == dto.StopName.ToLower() && s.StopId != id);
        if (duplicate)
            return Conflict(new { message = "A stop with this name already exists" });

        stop.StopName = dto.StopName;
        stop.Street = dto.Street;
        stop.Suburb = dto.Suburb;
        stop.City = dto.City;
        stop.PostalCode = dto.PostalCode;
        stop.GpsCoordinates = dto.GpsCoordinates;
        stop.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();

        return Ok(new StopResponseDto
        {
            StopId = stop.StopId,
            StopName = stop.StopName,
            Street = stop.Street,
            Suburb = stop.Suburb,
            City = stop.City,
            PostalCode = stop.PostalCode,
            FullAddress = stop.FullAddress,
            GpsCoordinates = stop.GpsCoordinates,
            IsActive = stop.IsActive
        });
    }

    /// <summary>
    /// Delete a stop. Rejects if TripInstances reference the stop (HTTP 409).
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var stop = await _context.Stops.FindAsync(id);
        if (stop == null)
            return NotFound(new { message = "Stop not found" });

        var hasTrips = await _context.TripInstances.AnyAsync(t => t.StopId == id);
        if (hasTrips)
            return Conflict(new { message = "Cannot delete stop: it is referenced by existing trip instances" });

        _context.Stops.Remove(stop);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static bool TryParseGpsCoordinates(string gps, out double latitude, out double longitude)
    {
        latitude = 0;
        longitude = 0;

        var parts = gps.Split(',');
        if (parts.Length != 2)
            return false;

        if (!double.TryParse(parts[0].Trim(), System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out latitude))
            return false;

        if (!double.TryParse(parts[1].Trim(), System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out longitude))
            return false;

        if (latitude < -90 || latitude > 90)
            return false;

        if (longitude < -180 || longitude > 180)
            return false;

        return true;
    }

    // ──────────────────────────────────────────────────────────────────
    // Default Convoy Endpoints
    // ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Get the default convoy for a stop, ordered by OrderPosition.
    /// </summary>
    [HttpGet("{stopId}/default-convoy")]
    public async Task<IActionResult> GetDefaultConvoy(int stopId)
    {
        var defaults = await _defaultConvoyService.GetDefaultConvoyAsync(stopId);

        var response = defaults.Select(d => new
        {
            d.StopShuttleDefaultId,
            d.StopId,
            d.ShuttleId,
            d.Shuttle.LicensePlate,
            d.Shuttle.IsAccessibleVehicle,
            d.Shuttle.Capacity,
            d.OrderPosition,
            d.CreatedAt
        });

        return Ok(response);
    }

    /// <summary>
    /// Add a shuttle to a stop's default convoy.
    /// </summary>
    [HttpPost("{stopId}/default-convoy")]
    public async Task<IActionResult> AddToDefaultConvoy(int stopId, [FromBody] AddToDefaultConvoyDto dto)
    {
        try
        {
            var record = await _defaultConvoyService.AssignShuttleToDefaultAsync(stopId, dto.ShuttleId);

            // Reload with shuttle nav for response
            var shuttle = await _context.Shuttles.FindAsync(record.ShuttleId);

            return CreatedAtAction(nameof(GetDefaultConvoy), new { stopId }, new
            {
                record.StopShuttleDefaultId,
                record.StopId,
                record.ShuttleId,
                LicensePlate = shuttle?.LicensePlate ?? "",
                IsAccessibleVehicle = shuttle?.IsAccessibleVehicle ?? false,
                Capacity = shuttle?.Capacity ?? 0,
                record.OrderPosition,
                record.CreatedAt
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("inactive"))
        {
            return UnprocessableEntity(new { message = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already"))
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Remove a shuttle from a stop's default convoy.
    /// </summary>
    [HttpDelete("{stopId}/default-convoy/{shuttleId}")]
    public async Task<IActionResult> RemoveFromDefaultConvoy(int stopId, int shuttleId)
    {
        try
        {
            await _defaultConvoyService.RemoveShuttleFromDefaultAsync(stopId, shuttleId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Reorder shuttles in a stop's default convoy.
    /// </summary>
    [HttpPatch("{stopId}/default-convoy/reorder")]
    public async Task<IActionResult> ReorderDefaultConvoy(int stopId, [FromBody] ReorderDefaultConvoyDto dto)
    {
        try
        {
            await _defaultConvoyService.ReorderDefaultAsync(stopId, dto.OrderedShuttleIds);
            var defaults = await _defaultConvoyService.GetDefaultConvoyAsync(stopId);

            var response = defaults.Select(d => new
            {
                d.StopShuttleDefaultId,
                d.StopId,
                d.ShuttleId,
                d.Shuttle.LicensePlate,
                d.Shuttle.IsAccessibleVehicle,
                d.Shuttle.Capacity,
                d.OrderPosition,
                d.CreatedAt
            });

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return UnprocessableEntity(new { message = ex.Message });
        }
    }

    // ──────────────────────────────────────────────────────────────────
    // Stop-Centric Trip View
    // ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// List trips for a stop with convoy summary, filterable by date range and status.
    /// </summary>
    [HttpGet("{stopId}/trips")]
    public async Task<IActionResult> GetTripsForStop(
        int stopId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] string? status)
    {
        var stop = await _context.Stops.FindAsync(stopId);
        if (stop == null)
            return NotFound(new { message = "Stop not found" });

        var sastToday = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(2));
        var effectiveFrom = fromDate ?? sastToday;
        var effectiveTo = toDate ?? sastToday.AddDays(3);

        var query = _context.TripInstances
            .Include(t => t.ScheduleTime)
            .Include(t => t.TripShuttleAllocations)
                .ThenInclude(a => a.Shuttle)
            .Where(t => t.StopId == stopId && t.TripDate >= effectiveFrom && t.TripDate <= effectiveTo);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(t => t.Status == status);

        var trips = await query
            .OrderBy(t => t.TripDate)
            .ThenBy(t => t.ScheduleTime.DepartureTime)
            .ToListAsync();

        var response = new List<object>();

        foreach (var trip in trips)
        {
            var shuttleCount = trip.TripShuttleAllocations.Count;
            var totalCapacity = trip.TripShuttleAllocations
                .Where(a => a.IsOperational)
                .Sum(a => a.Shuttle.Capacity);
            var confirmedCount = await _context.Bookings
                .CountAsync(b => b.TripInstanceId == trip.TripInstanceId && b.BookingStatus == "Confirmed");

            response.Add(new
            {
                tripInstanceId = trip.TripInstanceId,
                tripDate = trip.TripDate,
                departureTime = trip.ScheduleTime.DepartureTime,
                direction = trip.ScheduleTime.Direction,
                status = trip.Status,
                shuttleCount,
                totalCapacity,
                confirmedCount,
                hasEmptyConvoy = shuttleCount == 0
            });
        }

        return Ok(new
        {
            stopId,
            stopName = stop.StopName,
            trips = response
        });
    }
}
