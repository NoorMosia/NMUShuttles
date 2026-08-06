namespace NmuShuttle.Api.Services;

/// <summary>
/// Handles OTP generation, hashing, and verification for Student and Marshal authentication.
/// </summary>
public interface IOtpService
{
    /// <summary>
    /// Generates a 5-digit numeric OTP string (10000–99999).
    /// </summary>
    string GenerateOtp();

    /// <summary>
    /// Returns the SHA-256 hex hash of the given OTP.
    /// </summary>
    string HashOtp(string otp);

    /// <summary>
    /// Generates an OTP for the student, stores the hash + expiry, and logs the code.
    /// Returns false if student not found.
    /// </summary>
    Task<bool> RequestStudentOtpAsync(string studentNumber);

    /// <summary>
    /// Verifies the OTP for a student. Returns a JWT if valid, null otherwise.
    /// OTP fields are always cleared after verification (single-use).
    /// </summary>
    Task<string?> VerifyStudentOtpAsync(string studentNumber, string otp);

    /// <summary>
    /// Generates an OTP for the marshal, stores the hash + expiry, and logs the code.
    /// Returns false if marshal not found.
    /// </summary>
    Task<bool> RequestMarshalOtpAsync(string staffNumber);

    /// <summary>
    /// Verifies the OTP for a marshal. Returns a JWT if valid, null otherwise.
    /// OTP fields are always cleared after verification (single-use).
    /// </summary>
    Task<string?> VerifyMarshalOtpAsync(string staffNumber, string otp);

    /// <summary>
    /// Generates an OTP for the admin, stores the hash + expiry, and logs the code.
    /// Returns false if admin not found.
    /// </summary>
    Task<bool> RequestAdminOtpAsync(string email);

    /// <summary>
    /// Verifies the OTP for an admin. Returns a JWT if valid, null otherwise.
    /// OTP fields are always cleared after verification (single-use).
    /// </summary>
    Task<string?> VerifyAdminOtpAsync(string email, string otp);
}
