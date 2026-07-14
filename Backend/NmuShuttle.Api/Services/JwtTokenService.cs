using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace NmuShuttle.Api.Services;

/// <summary>
/// Issues HS256-signed JWT tokens with role-specific claims and expiry.
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;

        _secret = _configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("JWT Secret is not configured.");
        _issuer = _configuration["Jwt:Issuer"]
            ?? "NmuShuttle.Api";
        _audience = _configuration["Jwt:Audience"]
            ?? "NmuShuttle.Clients";
    }

    /// <inheritdoc />
    public string GenerateStudentToken(int studentId, string studentNumber, string firstName, string lastName, string email)
    {
        var expiryHours = _configuration.GetValue<int>("Jwt:StudentTokenExpiryHours", 24);
        var additionalClaims = new[]
        {
            new Claim("firstName", firstName),
            new Claim("lastName", lastName),
            new Claim("email", email),
            new Claim("studentNumber", studentNumber)
        };
        return GenerateToken(studentId.ToString(), "Student", studentNumber, TimeSpan.FromHours(expiryHours), additionalClaims);
    }

    /// <inheritdoc />
    public string GenerateMarshalToken(int marshalId, string staffNumber)
    {
        var expiryHours = _configuration.GetValue<int>("Jwt:MarshalTokenExpiryHours", 24);
        return GenerateToken(marshalId.ToString(), "Marshal", staffNumber, TimeSpan.FromHours(expiryHours));
    }

    /// <inheritdoc />
    public string GenerateAdminToken(int adminId, string email)
    {
        var expiryHours = _configuration.GetValue<int>("Jwt:AdminTokenExpiryHours", 8);
        return GenerateToken(adminId.ToString(), "Admin", email, TimeSpan.FromHours(expiryHours));
    }

    private string GenerateToken(string userId, string role, string identifier, TimeSpan expiry, Claim[]? additionalClaims = null)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var now = DateTime.UtcNow;

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(ClaimTypes.Role, role),
            new Claim("identifier", identifier),
            new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (additionalClaims != null)
            claims.AddRange(additionalClaims);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            notBefore: now,
            expires: now.Add(expiry),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
