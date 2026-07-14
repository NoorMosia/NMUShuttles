using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NmuShuttle.Api.Data;

namespace NmuShuttle.Api.Controllers;

/// <summary>
/// Endpoint for the authenticated marshal to retrieve their assignment info.
/// </summary>
[ApiController]
[Route("api/v1/marshal")]
[Authorize(Roles = "Marshal")]
public class MarshalProfileController : ControllerBase
{
    private readonly ShuttleDbContext _db;

    public MarshalProfileController(ShuttleDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Returns the authenticated marshal's assignment (campus or stop) with GPS coordinates.
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyAssignment()
    {
        var marshalId = GetMarshalId();

        var marshal = await _db.Marshals
            .AsNoTracking()
            .Include(m => m.AssignedCampus)
            .Include(m => m.AssignedStop)
            .FirstOrDefaultAsync(m => m.MarshalId == marshalId);

        if (marshal == null)
            return NotFound();

        return Ok(new
        {
            marshalId = marshal.MarshalId,
            staffNumber = marshal.StaffNumber,
            firstName = marshal.FirstName,
            lastName = marshal.LastName,
            assignedCampusId = marshal.AssignedCampusId,
            assignedCampusName = marshal.AssignedCampus?.CampusName,
            assignedStopId = marshal.AssignedStopId,
            assignedStopName = marshal.AssignedStop?.StopName,
            gpsCoordinates = marshal.AssignedCampus?.GpsCoordinates
                ?? marshal.AssignedStop?.GpsCoordinates,
        });
    }

    private int GetMarshalId()
    {
        var subClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                       ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.Parse(subClaim!);
    }
}
