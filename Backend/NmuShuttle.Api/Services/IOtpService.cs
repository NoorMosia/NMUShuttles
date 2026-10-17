namespace NmuShuttle.Api.Services;

/// <summary>
/// Handles OTP generation, hashing, and verification for Student and Marshal authentication.
/// </summary>
public interface IOtpService
{
    /// <summary>
    /// Whether OTP codes may be returned to the client in the API response.
    /// True in non-Production builds, or when the Otp:ExposeInResponse flag is set.
    /// Intended for evaluation only — never enable for real production use.
    /// </summary>
    bool ExposeOtpInResponse { get; }

    /// <summary>
    /// Generates a 5-digit numeric OTP string (10000–99999).
    /// </summary>
    string GenerateOtp();

    /// <summary>
    /// Generates and stores an OTP for a student, returning the plain code.
    /// Returns null if the student does not exist. The plain OTP is intended
    /// for non-production evaluation builds only — never expose it in Production.
    /// </summary>
    Task<string?> RequestStudentOtpWithCodeAsync(string studentNumber);

    /// <summary>
    /// Generates and stores an OTP for a marshal, returning the plain code.
    /// Returns null if the marshal does not exist. The plain OTP is intended
    /// for non-production evaluation builds only — never expose it in Production.
    /// </summary>
    Task<string?> RequestMarshalOtpWithCodeAsync(string staffNumber);

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
    /// Generates and stores an OTP for an admin, returning the plain code.
    /// Returns null if the admin does not exist. The plain OTP is intended
    /// for evaluation only — never expose it in real production use.
    /// </summary>
    Task<string?> RequestAdminOtpWithCodeAsync(string email);

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
