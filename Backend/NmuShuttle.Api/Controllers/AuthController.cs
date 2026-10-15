using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using NmuShuttle.Api.Data;
using NmuShuttle.Api.DTOs;
using NmuShuttle.Api.Models;
using NmuShuttle.Api.Services;

namespace NmuShuttle.Api.Controllers;

/// <summary>
/// Handles OTP-based authentication for Students and Marshals,
/// and device registration for push notifications.
/// </summary>
[ApiController]
[Route("api/v1/auth")]
public partial class AuthController : ControllerBase
{
    private readonly IOtpService _otpService;
    private readonly ShuttleDbContext _dbContext;
    private readonly IHostEnvironment _environment;

    public AuthController(IOtpService otpService, ShuttleDbContext dbContext, IHostEnvironment environment)
    {
        _otpService = otpService;
        _dbContext = dbContext;
        _environment = environment;
    }

    // ──────────────────────────────────────────────────────────────
    // Student OTP endpoints
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Request an OTP for a student. The OTP is logged in development mode.
    /// </summary>
    [HttpPost("student/otp/request")]
    [EnableRateLimiting("OtpRateLimit")]
    public async Task<IActionResult> RequestStudentOtp([FromBody] StudentOtpRequestDto dto)
    {
        if (!StudentNumberRegex().IsMatch(dto.StudentNumber))
        {
            return UnprocessableEntity(new ProblemDetails
            {
                Status = StatusCodes.Status422UnprocessableEntity,
                Title = "Validation Error",
                Detail = "StudentNumber must match format: s3 followed by 1 or 2, then 7 digits (e.g., s312345678).",
                Instance = HttpContext.Request.Path
            });
        }

        var otp = await _otpService.RequestStudentOtpWithCodeAsync(dto.StudentNumber);

        if (otp is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found",
                Detail = "Student not found.",
                Instance = HttpContext.Request.Path
            });
        }

        // EVALUATION ONLY: expose the OTP to the client in non-production builds so
        // testers can log in without inbox access. Never returned in Production.
        return Ok(new
        {
            Message = "OTP sent successfully.",
            DevOtp = _environment.IsProduction() ? null : otp
        });
    }

    /// <summary>
    /// Verify a student's OTP and return a JWT on success.
    /// </summary>
    [HttpPost("student/otp/verify")]
    public async Task<IActionResult> VerifyStudentOtp([FromBody] StudentOtpVerifyDto dto)
    {
        var token = await _otpService.VerifyStudentOtpAsync(dto.StudentNumber, dto.Otp);

        if (token is null)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = "OTP is invalid or expired.",
                Instance = HttpContext.Request.Path
            });
        }

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        var expiresAt = jwt.ValidTo;

        return Ok(new AuthResponseDto(token, expiresAt, "Student"));
    }

    // ──────────────────────────────────────────────────────────────
    // Marshal OTP endpoints
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Request an OTP for a marshal. The OTP is logged in development mode.
    /// </summary>
    [HttpPost("marshal/otp/request")]
    [EnableRateLimiting("OtpRateLimit")]
    public async Task<IActionResult> RequestMarshalOtp([FromBody] MarshalOtpRequestDto dto)
    {
        if (!StaffNumberRegex().IsMatch(dto.StaffNumber))
        {
            return UnprocessableEntity(new ProblemDetails
            {
                Status = StatusCodes.Status422UnprocessableEntity,
                Title = "Validation Error",
                Detail = "StaffNumber must be exactly 7 digits.",
                Instance = HttpContext.Request.Path
            });
        }

        var otp = await _otpService.RequestMarshalOtpWithCodeAsync(dto.StaffNumber);

        if (otp is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found",
                Detail = "Marshal not found.",
                Instance = HttpContext.Request.Path
            });
        }

        // EVALUATION ONLY: expose the OTP to the client in non-production builds so
        // testers can log in without inbox access. Never returned in Production.
        return Ok(new
        {
            Message = "OTP sent successfully.",
            DevOtp = _environment.IsProduction() ? null : otp
        });
    }

    /// <summary>
    /// Verify a marshal's OTP and return a JWT on success.
    /// </summary>
    [HttpPost("marshal/otp/verify")]
    public async Task<IActionResult> VerifyMarshalOtp([FromBody] MarshalOtpVerifyDto dto)
    {
        var token = await _otpService.VerifyMarshalOtpAsync(dto.StaffNumber, dto.Otp);

        if (token is null)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = "OTP is invalid or expired.",
                Instance = HttpContext.Request.Path
            });
        }

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        var expiresAt = jwt.ValidTo;

        return Ok(new AuthResponseDto(token, expiresAt, "Marshal"));
    }

    // ──────────────────────────────────────────────────────────────
    // Device Registration endpoint
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Register or update a device for push notifications (FCM).
    /// Upserts by (OwnerId, OwnerType, Platform).
    /// </summary>
    [HttpPost("device/register")]
    [Authorize(Roles = "Student,Marshal")]
    public async Task<IActionResult> RegisterDevice([FromBody] DeviceRegistrationDto dto)
    {
        // Validate platform
        var platform = dto.Platform?.ToLowerInvariant();
        if (platform is not ("android" or "ios"))
        {
            return UnprocessableEntity(new ProblemDetails
            {
                Status = StatusCodes.Status422UnprocessableEntity,
                Title = "Validation Error",
                Detail = "Platform must be 'android' or 'ios'.",
                Instance = HttpContext.Request.Path
            });
        }

        // Validate FcmToken is not empty
        if (string.IsNullOrWhiteSpace(dto.FcmToken))
        {
            return UnprocessableEntity(new ProblemDetails
            {
                Status = StatusCodes.Status422UnprocessableEntity,
                Title = "Validation Error",
                Detail = "FcmToken is required.",
                Instance = HttpContext.Request.Path
            });
        }

        // Extract userId and role from JWT claims
        var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                          ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        var roleClaim = User.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var ownerId))
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = "Invalid or missing user identity claim.",
                Instance = HttpContext.Request.Path
            });
        }

        var ownerType = roleClaim ?? "Student";

        // Upsert: look up existing registration by (OwnerId, OwnerType, Platform)
        var existing = await _dbContext.DeviceRegistrations
            .FirstOrDefaultAsync(d =>
                d.OwnerId == ownerId &&
                d.OwnerType == ownerType &&
                d.Platform == platform);

        if (existing is not null)
        {
            // Update existing registration
            existing.FcmToken = dto.FcmToken;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            // Create new registration
            var registration = new DeviceRegistration
            {
                OwnerId = ownerId,
                OwnerType = ownerType,
                FcmToken = dto.FcmToken,
                Platform = platform,
                UpdatedAt = DateTime.UtcNow
            };
            _dbContext.DeviceRegistrations.Add(registration);
        }

        await _dbContext.SaveChangesAsync();

        return Ok(new { Message = "Device registered successfully." });
    }

    // ──────────────────────────────────────────────────────────────
    // Compiled regex patterns for format validation
    // ──────────────────────────────────────────────────────────────

    [GeneratedRegex(@"^s3[12]\d{7}$")]
    private static partial Regex StudentNumberRegex();

    [GeneratedRegex(@"^\d{7}$")]
    private static partial Regex StaffNumberRegex();
}
