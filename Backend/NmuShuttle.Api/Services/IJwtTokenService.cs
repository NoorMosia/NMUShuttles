namespace NmuShuttle.Api.Services;

/// <summary>
/// Issues signed JWT tokens for authenticated users.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a JWT for an authenticated Student (24-hour expiry, role=Student).
    /// </summary>
    string GenerateStudentToken(int studentId, string studentNumber);

    /// <summary>
    /// Generates a JWT for an authenticated Marshal (24-hour expiry, role=Marshal).
    /// </summary>
    string GenerateMarshalToken(int marshalId, string staffNumber);

    /// <summary>
    /// Generates a JWT for an authenticated Admin (8-hour expiry, role=Admin).
    /// </summary>
    string GenerateAdminToken(int adminId, string email);
}
