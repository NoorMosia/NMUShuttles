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
}
