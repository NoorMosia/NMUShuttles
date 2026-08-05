using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NmuShuttle.Api.Data;
using NmuShuttle.Api.Models;

namespace NmuShuttle.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/vendors")]
[Authorize(Roles = "Admin")]
public class VendorsAdminController : ControllerBase
{
    private readonly ShuttleDbContext _context;

    public VendorsAdminController(ShuttleDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// List all vendors.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var vendors = await _context.Vendors
            .OrderBy(v => v.CompanyName)
            .Select(v => new VendorResponseDto
            {
                VendorId = v.VendorId,
                CompanyName = v.CompanyName,
                ContactEmail = v.ContactEmail,
                ContactPhone = v.ContactPhone,
                ShuttleCount = v.Shuttles.Count,
                CreatedAt = v.CreatedAt
            })
            .ToListAsync();

        return Ok(vendors);
    }

    /// <summary>
    /// Get a single vendor by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var vendor = await _context.Vendors
            .Include(v => v.Shuttles)
            .FirstOrDefaultAsync(v => v.VendorId == id);

        if (vendor == null)
            return NotFound(new { message = "Vendor not found" });

        return Ok(new VendorResponseDto
        {
            VendorId = vendor.VendorId,
            CompanyName = vendor.CompanyName,
            ContactEmail = vendor.ContactEmail,
            ContactPhone = vendor.ContactPhone,
            ShuttleCount = vendor.Shuttles.Count,
            CreatedAt = vendor.CreatedAt
        });
    }

    /// <summary>
    /// Create a new vendor.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] VendorCreateDto dto)
    {
        // Check for duplicate company name
        var exists = await _context.Vendors
            .AnyAsync(v => v.CompanyName == dto.CompanyName);
        if (exists)
            return Conflict(new { message = "A vendor with this company name already exists" });

        var vendor = new Vendor
        {
            CompanyName = dto.CompanyName,
            ContactEmail = dto.ContactEmail,
            ContactPhone = dto.ContactPhone,
            CreatedAt = DateTime.UtcNow
        };

        _context.Vendors.Add(vendor);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = vendor.VendorId }, new VendorResponseDto
        {
            VendorId = vendor.VendorId,
            CompanyName = vendor.CompanyName,
            ContactEmail = vendor.ContactEmail,
            ContactPhone = vendor.ContactPhone,
            ShuttleCount = 0,
            CreatedAt = vendor.CreatedAt
        });
    }

    /// <summary>
    /// Update an existing vendor.
    /// </summary>
    [HttpPatch("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] VendorUpdateDto dto)
    {
        var vendor = await _context.Vendors.FindAsync(id);
        if (vendor == null)
            return NotFound(new { message = "Vendor not found" });

        // Check for duplicate company name (excluding current vendor)
        if (dto.CompanyName != null && dto.CompanyName != vendor.CompanyName)
        {
            var exists = await _context.Vendors
                .AnyAsync(v => v.CompanyName == dto.CompanyName && v.VendorId != id);
            if (exists)
                return Conflict(new { message = "A vendor with this company name already exists" });

            vendor.CompanyName = dto.CompanyName;
        }

        if (dto.ContactEmail != null)
            vendor.ContactEmail = dto.ContactEmail;

        if (dto.ContactPhone != null)
            vendor.ContactPhone = dto.ContactPhone;

        await _context.SaveChangesAsync();

        var shuttleCount = await _context.Shuttles.CountAsync(s => s.VendorId == id);

        return Ok(new VendorResponseDto
        {
            VendorId = vendor.VendorId,
            CompanyName = vendor.CompanyName,
            ContactEmail = vendor.ContactEmail,
            ContactPhone = vendor.ContactPhone,
            ShuttleCount = shuttleCount,
            CreatedAt = vendor.CreatedAt
        });
    }

    /// <summary>
    /// Delete a vendor (only if no shuttles reference it).
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var vendor = await _context.Vendors.FindAsync(id);
        if (vendor == null)
            return NotFound(new { message = "Vendor not found" });

        var hasShuttles = await _context.Shuttles.AnyAsync(s => s.VendorId == id);
        if (hasShuttles)
            return UnprocessableEntity(new { message = "Cannot delete vendor with assigned shuttles. Reassign shuttles first." });

        _context.Vendors.Remove(vendor);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

// ─── DTOs ────────────────────────────────────────────────────────────────────

public class VendorResponseDto
{
    public int VendorId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public int ShuttleCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class VendorCreateDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
}

public class VendorUpdateDto
{
    public string? CompanyName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
}
