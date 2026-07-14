using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NmuShuttle.Api.Data;
using NmuShuttle.Api.DTOs;
using NmuShuttle.Api.Services;

namespace NmuShuttle.Api.Controllers;

[ApiController]
[Route("api/v1/auth/admin")]
public class AdminAuthController : ControllerBase
{
    private readonly ShuttleDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IConfiguration _configuration;

    public AdminAuthController(
        ShuttleDbContext context,
        IJwtTokenService jwtTokenService,
        IConfiguration configuration)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
        _configuration = configuration;
    }

    /// <summary>
    /// Authenticates an admin using email and password.
    /// Returns a JWT token on success, or HTTP 401 with a generic message on failure.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AdminLoginDto dto)
    {
        // Look up admin by email (case-insensitive)
        var admin = await _context.Admins
            .FirstOrDefaultAsync(a => a.Email.ToLower() == dto.Email.ToLower());

        // If not found, return generic 401 — do NOT disclose which field was wrong
        if (admin == null)
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }

        // DEV BYPASS: skip password check in Development environment
        // TODO: Remove this before production deployment
        var env = HttpContext.RequestServices.GetRequiredService<IHostEnvironment>();
        bool passwordValid;
        if (env.IsDevelopment())
        {
            passwordValid = true; // Skip BCrypt verification in dev
        }
        else
        {
            passwordValid = BCrypt.Net.BCrypt.Verify(dto.Password, admin.PasswordHash);
        }

        if (!passwordValid)
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }

        // Generate admin JWT (8-hour expiry configured in JwtTokenService)
        var token = _jwtTokenService.GenerateAdminToken(admin.AdminId, admin.Email);

        // Calculate expiry for response
        var expiryHours = _configuration.GetValue<int>("Jwt:AdminTokenExpiryHours", 8);
        var expiresAt = DateTime.UtcNow.AddHours(expiryHours);

        return Ok(new AdminLoginResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            Role = "Admin"
        });
    }
}
