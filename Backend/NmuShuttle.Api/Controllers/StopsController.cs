using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NmuShuttle.Api.Data;

namespace NmuShuttle.Api.Controllers;

/// <summary>
/// Public stops endpoint accessible to all authenticated users (Student, Marshal, Admin).
/// Used by the Maps screen to display stop markers.
/// </summary>
[ApiController]
[Route("api/v1/stops")]
[Authorize]
public class StopsController : ControllerBase
{
    private readonly ShuttleDbContext _context;

    public StopsController(ShuttleDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Returns all active stops with GPS coordinates.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var stops = await _context.Stops
            .Where(s => s.IsActive)
            .Select(s => new
            {
                s.StopId,
                s.StopName,
                s.FullAddress,
                s.GpsCoordinates,
                s.IsActive
            })
            .ToListAsync();

        return Ok(stops);
    }
}
