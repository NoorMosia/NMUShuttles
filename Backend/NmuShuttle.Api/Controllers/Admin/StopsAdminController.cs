using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NmuShuttle.Api.Data;
using NmuShuttle.Api.DTOs.Admin;
using NmuShuttle.Api.Models;

namespace NmuShuttle.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/stops")]
[Authorize(Roles = "Admin")]
public class StopsAdminController : ControllerBase
{
    private readonly ShuttleDbContext _context;

    public StopsAdminController(ShuttleDbContext context)
    {
        _context = context;
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
                FullAddress = s.FullAddress,
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
            FullAddress = stop.FullAddress,
            GpsCoordinates = stop.GpsCoordinates,
            IsActive = stop.IsActive
        });
    }

    /// <summary>
    /// Create a new stop. Validates StopName, FullAddress, GpsCoordinates non-empty;
    /// validates GPS format; rejects duplicate StopName with 409.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStopDto dto)
    {
        // Validate required fields non-empty
        if (string.IsNullOrWhiteSpace(dto.StopName))
            return UnprocessableEntity(new { message = "StopName is required" });
        if (string.IsNullOrWhiteSpace(dto.FullAddress))
            return UnprocessableEntity(new { message = "FullAddress is required" });
        if (string.IsNullOrWhiteSpace(dto.GpsCoordinates))
            return UnprocessableEntity(new { message = "GpsCoordinates is required" });

        // Validate GPS format
        if (!TryParseGpsCoordinates(dto.GpsCoordinates, out _, out _))
            return UnprocessableEntity(new { message = "Invalid GPS coordinates. Expected format: 'lat,lon' with lat in [-90,90] and lon in [-180,180]" });

        // Reject duplicate StopName
        var duplicate = await _context.Stops
            .AnyAsync(s => s.StopName.ToLower() == dto.StopName.ToLower());
        if (duplicate)
            return Conflict(new { message = "A stop with this name already exists" });

        var stop = new Stop
        {
            StopName = dto.StopName,
            FullAddress = dto.FullAddress,
            GpsCoordinates = dto.GpsCoordinates,
            IsActive = dto.IsActive
        };

        _context.Stops.Add(stop);
        await _context.SaveChangesAsync();

        var response = new StopResponseDto
        {
            StopId = stop.StopId,
            StopName = stop.StopName,
            FullAddress = stop.FullAddress,
            GpsCoordinates = stop.GpsCoordinates,
            IsActive = stop.IsActive
        };

        return CreatedAtAction(nameof(GetById), new { id = stop.StopId }, response);
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

        // Validate required fields non-empty
        if (string.IsNullOrWhiteSpace(dto.StopName))
            return UnprocessableEntity(new { message = "StopName is required" });
        if (string.IsNullOrWhiteSpace(dto.FullAddress))
            return UnprocessableEntity(new { message = "FullAddress is required" });
        if (string.IsNullOrWhiteSpace(dto.GpsCoordinates))
            return UnprocessableEntity(new { message = "GpsCoordinates is required" });

        // Validate GPS format
        if (!TryParseGpsCoordinates(dto.GpsCoordinates, out _, out _))
            return UnprocessableEntity(new { message = "Invalid GPS coordinates. Expected format: 'lat,lon' with lat in [-90,90] and lon in [-180,180]" });

        // Reject duplicate StopName (exclude current stop)
        var duplicate = await _context.Stops
            .AnyAsync(s => s.StopName.ToLower() == dto.StopName.ToLower() && s.StopId != id);
        if (duplicate)
            return Conflict(new { message = "A stop with this name already exists" });

        stop.StopName = dto.StopName;
        stop.FullAddress = dto.FullAddress;
        stop.GpsCoordinates = dto.GpsCoordinates;
        stop.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();

        return Ok(new StopResponseDto
        {
            StopId = stop.StopId,
            StopName = stop.StopName,
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

        // Check referential integrity — reject if TripInstances reference this stop
        var hasTrips = await _context.TripInstances.AnyAsync(t => t.StopId == id);
        if (hasTrips)
            return Conflict(new { message = "Cannot delete stop: it is referenced by existing trip instances" });

        _context.Stops.Remove(stop);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Parses GPS coordinates in "lat,lon" format and validates ranges.
    /// </summary>
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
}
