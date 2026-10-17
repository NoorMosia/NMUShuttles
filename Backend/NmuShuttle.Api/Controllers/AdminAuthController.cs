using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NmuShuttle.Api.DTOs;
using NmuShuttle.Api.Services;

namespace NmuShuttle.Api.Controllers;

[ApiController]
[Route("api/v1/auth/admin")]
public class AdminAuthController : ControllerBase
{
    private readonly IOtpService _otpService;
    private readonly IConfiguration _configuration;

    public AdminAuthController(
        IOtpService otpService,
        IConfiguration configuration)
    {
        _otpService = otpService;
        _configuration = configuration;
    }

    /// <summary>
    /// Request an OTP for an admin. The OTP is logged in development mode.
    /// </summary>
    [HttpPost("otp/request")]
    [EnableRateLimiting("OtpRateLimit")]
    public async Task<IActionResult> RequestOtp([FromBody] AdminOtpRequestDto dto)
    {
        var otp = await _otpService.RequestAdminOtpWithCodeAsync(dto.Email);

        if (otp is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found",
                Detail = "Admin not found.",
                Instance = HttpContext.Request.Path
            });
        }

        // EVALUATION ONLY: expose the OTP to the client when enabled so testers can
        // log in without inbox access. Controlled by Otp:ExposeInResponse / environment.
        return Ok(new
        {
            Message = "OTP sent successfully.",
            DevOtp = _otpService.ExposeOtpInResponse ? otp : null
        });
    }

    /// <summary>
    /// Verify an admin's OTP and return a JWT on success.
    /// </summary>
    [HttpPost("otp/verify")]
    public async Task<IActionResult> VerifyOtp([FromBody] AdminOtpVerifyDto dto)
    {
        var token = await _otpService.VerifyAdminOtpAsync(dto.Email, dto.Otp);

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

        return Ok(new AdminLoginResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            Role = "Admin"
        });
    }
}
