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
    private readonly IEmailService _emailService;
    private readonly ILogger<OtpService> _logger;
    private readonly IHostEnvironment _environment;

    public OtpService(
        ShuttleDbContext db,
        IJwtTokenService jwtTokenService,
        IEmailService emailService,
        ILogger<OtpService> logger,
        IHostEnvironment environment)
    {
        _db = db;
        _jwtTokenService = jwtTokenService;
        _emailService = emailService;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Sends the OTP by email. In Development, email failures are logged but do not
    /// fail the request (the code is also written to the console). Outside Development,
    /// a send failure propagates so the caller can surface it.
    /// </summary>
    private async Task SendOtpEmailAsync(string toEmail, string otp, string? recipientName)
    {
        // TEMP: Resend's test sender (onboarding@resend.dev) only delivers to the
        // account's own verified address. Until a sending domain is verified in Resend,
        // route every OTP email to this fixed inbox regardless of the user's record email.
        // Remove this override and use `toEmail` once a domain is verified.
        const string overrideRecipient = "s229895735@mandela.ac.za";

        try
        {
            await _emailService.SendOtpAsync(overrideRecipient, otp, recipientName);
        }
        catch (Exception ex) when (_environment.IsDevelopment())
        {
            _logger.LogWarning(ex,
                "OTP email send failed in Development for {Email}; relying on console log.", overrideRecipient);
        }
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
    public async Task<string?> RequestStudentOtpWithCodeAsync(string studentNumber)
    {
        var student = await _db.Students
            .FirstOrDefaultAsync(s => s.StudentNumber == studentNumber);

        if (student is null)
            return null;

        var otp = GenerateOtp();
        student.OtpHash = HashOtp(otp);
        student.OtpExpiry = DateTime.UtcNow.AddMinutes(5);

        await _db.SaveChangesAsync();

        _logger.LogInformation("OTP for {Identifier}: {Otp}", studentNumber, otp);

        await SendOtpEmailAsync(student.UniversityEmail, otp, student.FirstName);

        return otp;
    }

    /// <inheritdoc />
    public async Task<bool> RequestStudentOtpAsync(string studentNumber)
        => await RequestStudentOtpWithCodeAsync(studentNumber) is not null;

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

        return _jwtTokenService.GenerateStudentToken(student.StudentId, student.StudentNumber, student.FirstName, student.LastName, student.UniversityEmail);
    }

    /// <inheritdoc />
    public async Task<string?> RequestMarshalOtpWithCodeAsync(string staffNumber)
    {
        var marshal = await _db.Marshals
            .FirstOrDefaultAsync(m => m.StaffNumber == staffNumber);

        if (marshal is null)
            return null;

        var otp = GenerateOtp();
        marshal.OtpHash = HashOtp(otp);
        marshal.OtpExpiry = DateTime.UtcNow.AddMinutes(5);

        await _db.SaveChangesAsync();

        _logger.LogInformation("OTP for {Identifier}: {Otp}", staffNumber, otp);

        await SendOtpEmailAsync(marshal.UniversityEmail, otp, marshal.FirstName);

        return otp;
    }

    /// <inheritdoc />
    public async Task<bool> RequestMarshalOtpAsync(string staffNumber)
        => await RequestMarshalOtpWithCodeAsync(staffNumber) is not null;

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

    /// <inheritdoc />
    public async Task<bool> RequestAdminOtpAsync(string email)
    {
        var admin = await _db.Admins
            .FirstOrDefaultAsync(a => a.Email.ToLower() == email.ToLower());

        if (admin is null)
            return false;

        var otp = GenerateOtp();
        admin.OtpHash = HashOtp(otp);
        admin.OtpExpiry = DateTime.UtcNow.AddMinutes(5);

        await _db.SaveChangesAsync();

        _logger.LogInformation("OTP for admin {Identifier}: {Otp}", email, otp);

        await SendOtpEmailAsync(admin.Email, otp, admin.FirstName);

        return true;
    }

    /// <inheritdoc />
    public async Task<string?> VerifyAdminOtpAsync(string email, string otp)
    {
        var admin = await _db.Admins
            .FirstOrDefaultAsync(a => a.Email.ToLower() == email.ToLower());

        if (admin is null)
            return null;

        var isValid = admin.OtpHash is not null
            && admin.OtpExpiry.HasValue
            && admin.OtpExpiry.Value > DateTime.UtcNow
            && admin.OtpHash == HashOtp(otp);

        // Always clear OTP fields (single-use)
        admin.OtpHash = null;
        admin.OtpExpiry = null;
        await _db.SaveChangesAsync();

        if (!isValid)
            return null;

        return _jwtTokenService.GenerateAdminToken(admin.AdminId, admin.Email);
    }
}
