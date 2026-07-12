using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using NmuShuttle.Api.Data;

namespace NmuShuttle.Api.Services;

/// <summary>
/// OTP service handling generation, hashing, storage, and verification for Students and Marshals.
/// OTPs are 5-digit numeric codes with a 5-minute expiry, stored as SHA-256 hashes.
/// </summary>
public class OtpService : IOtpService
{
    private readonly ShuttleDbContext _db;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<OtpService> _logger;
    private readonly IHostEnvironment _environment;

    public OtpService(
        ShuttleDbContext db,
        IJwtTokenService jwtTokenService,
        ILogger<OtpService> logger,
        IHostEnvironment environment)
    {
        _db = db;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
        _environment = environment;
    }

    /// <inheritdoc />
    public string GenerateOtp()
    {
        return Random.Shared.Next(10000, 99999).ToString("D5");
    }

    /// <inheritdoc />
    public string HashOtp(string otp)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(otp));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    /// <inheritdoc />
    public async Task<bool> RequestStudentOtpAsync(string studentNumber)
    {
        var student = await _db.Students
            .FirstOrDefaultAsync(s => s.StudentNumber == studentNumber);

        if (student is null)
            return false;

        var otp = GenerateOtp();
        student.OtpHash = HashOtp(otp);
        student.OtpExpiry = DateTime.UtcNow.AddMinutes(5);

        await _db.SaveChangesAsync();

        if (_environment.IsDevelopment())
        {
            _logger.LogInformation("OTP for {Identifier}: {Otp}", studentNumber, otp);
        }

        return true;
    }

    /// <inheritdoc />
    public async Task<string?> VerifyStudentOtpAsync(string studentNumber, string otp)
    {
        var student = await _db.Students
            .FirstOrDefaultAsync(s => s.StudentNumber == studentNumber);

        if (student is null)
            return null;

        var isValid = student.OtpHash is not null
            && student.OtpExpiry.HasValue
            && student.OtpExpiry.Value > DateTime.UtcNow
            && student.OtpHash == HashOtp(otp);

        // Always clear OTP fields (single-use)
        student.OtpHash = null;
        student.OtpExpiry = null;
        await _db.SaveChangesAsync();

        if (!isValid)
            return null;

        return _jwtTokenService.GenerateStudentToken(student.StudentId, student.StudentNumber);
    }

    /// <inheritdoc />
    public async Task<bool> RequestMarshalOtpAsync(string staffNumber)
    {
        var marshal = await _db.Marshals
            .FirstOrDefaultAsync(m => m.StaffNumber == staffNumber);

        if (marshal is null)
            return false;

        var otp = GenerateOtp();
        marshal.OtpHash = HashOtp(otp);
        marshal.OtpExpiry = DateTime.UtcNow.AddMinutes(5);

        await _db.SaveChangesAsync();

        if (_environment.IsDevelopment())
        {
            _logger.LogInformation("OTP for {Identifier}: {Otp}", staffNumber, otp);
        }

        return true;
    }

    /// <inheritdoc />
    public async Task<string?> VerifyMarshalOtpAsync(string staffNumber, string otp)
    {
        var marshal = await _db.Marshals
            .FirstOrDefaultAsync(m => m.StaffNumber == staffNumber);

        if (marshal is null)
            return null;

        var isValid = marshal.OtpHash is not null
            && marshal.OtpExpiry.HasValue
            && marshal.OtpExpiry.Value > DateTime.UtcNow
            && marshal.OtpHash == HashOtp(otp);

        // Always clear OTP fields (single-use)
        marshal.OtpHash = null;
        marshal.OtpExpiry = null;
        await _db.SaveChangesAsync();

        if (!isValid)
            return null;

        return _jwtTokenService.GenerateMarshalToken(marshal.MarshalId, marshal.StaffNumber);
    }
}
