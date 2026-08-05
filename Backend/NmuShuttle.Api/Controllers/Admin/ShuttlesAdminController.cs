using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NmuShuttle.Api.Data;
using NmuShuttle.Api.DTOs.Admin;
using NmuShuttle.Api.Models;

namespace NmuShuttle.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/shuttles")]
[Authorize(Roles = "Admin")]
public class ShuttlesAdminController : ControllerBase
{
    private readonly ShuttleDbContext _context;

    public ShuttlesAdminController(ShuttleDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// List all shuttles.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var shuttles = await _context.Shuttles
            .Include(s => s.Vendor)
            .Select(s => new ShuttleResponseDto
            {
                ShuttleId = s.ShuttleId,
                LicensePlate = s.LicensePlate,
                Capacity = s.Capacity,
                VendorId = s.VendorId,
                VendorCompanyName = s.Vendor.CompanyName,
                IsAccessibleVehicle = s.IsAccessibleVehicle,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync();

        return Ok(shuttles);
    }

    /// <summary>
    /// Get a single shuttle by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var shuttle = await _context.Shuttles
            .Include(s => s.Vendor)
            .FirstOrDefaultAsync(s => s.ShuttleId == id);

        if (shuttle == null)
            return NotFound(new { message = "Shuttle not found" });

        return Ok(new ShuttleResponseDto
        {
            ShuttleId = shuttle.ShuttleId,
            LicensePlate = shuttle.LicensePlate,
            Capacity = shuttle.Capacity,
            VendorId = shuttle.VendorId,
            VendorCompanyName = shuttle.Vendor.CompanyName,
            IsAccessibleVehicle = shuttle.IsAccessibleVehicle,
            CreatedAt = shuttle.CreatedAt
        });
    }

    /// <summary>
    /// Create a new shuttle. Validates Capacity > 0; rejects duplicate LicensePlate with 409.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateShuttleDto dto)
    {
        if (dto.Capacity <= 0)
            return UnprocessableEntity(new { message = "Capacity must be greater than 0" });

        // Validate vendor exists
        var vendor = await _context.Vendors.FindAsync(dto.VendorId);
        if (vendor == null)
            return UnprocessableEntity(new { message = "Invalid VendorId" });

        // Reject duplicate LicensePlate
        var duplicate = await _context.Shuttles
            .AnyAsync(s => s.LicensePlate.ToLower() == dto.LicensePlate.ToLower());
        if (duplicate)
            return Conflict(new { message = "A shuttle with this license plate already exists" });

        var shuttle = new Shuttle
        {
            LicensePlate = dto.LicensePlate,
            Capacity = dto.Capacity,
            VendorId = dto.VendorId,
            VendorCompanyName = vendor.CompanyName,
            IsAccessibleVehicle = dto.IsAccessibleVehicle,
            CreatedAt = DateTime.UtcNow
        };

        _context.Shuttles.Add(shuttle);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = shuttle.ShuttleId }, new ShuttleResponseDto
        {
            ShuttleId = shuttle.ShuttleId,
            LicensePlate = shuttle.LicensePlate,
            Capacity = shuttle.Capacity,
            VendorId = shuttle.VendorId,
            VendorCompanyName = vendor.CompanyName,
            IsAccessibleVehicle = shuttle.IsAccessibleVehicle,
            CreatedAt = shuttle.CreatedAt
        });
    }

    /// <summary>
    /// Update a shuttle.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateShuttleDto dto)
    {
        var shuttle = await _context.Shuttles.FindAsync(id);
        if (shuttle == null)
            return NotFound(new { message = "Shuttle not found" });

        if (dto.Capacity <= 0)
            return UnprocessableEntity(new { message = "Capacity must be greater than 0" });

        // Validate vendor exists
        var vendor = await _context.Vendors.FindAsync(dto.VendorId);
        if (vendor == null)
            return UnprocessableEntity(new { message = "Invalid VendorId" });

        // Reject duplicate LicensePlate (exclude current shuttle)
        var duplicate = await _context.Shuttles
            .AnyAsync(s => s.LicensePlate.ToLower() == dto.LicensePlate.ToLower() && s.ShuttleId != id);
        if (duplicate)
            return Conflict(new { message = "A shuttle with this license plate already exists" });

        shuttle.LicensePlate = dto.LicensePlate;
        shuttle.Capacity = dto.Capacity;
        shuttle.VendorId = dto.VendorId;
        shuttle.VendorCompanyName = vendor.CompanyName;
        shuttle.IsAccessibleVehicle = dto.IsAccessibleVehicle;

        await _context.SaveChangesAsync();

        return Ok(new ShuttleResponseDto
        {
            ShuttleId = shuttle.ShuttleId,
            LicensePlate = shuttle.LicensePlate,
            Capacity = shuttle.Capacity,
            VendorId = shuttle.VendorId,
            VendorCompanyName = vendor.CompanyName,
            IsAccessibleVehicle = shuttle.IsAccessibleVehicle,
            CreatedAt = shuttle.CreatedAt
        });
    }
}
