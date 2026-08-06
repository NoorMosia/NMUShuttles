using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NmuShuttle.Api.Data;

namespace NmuShuttle.Api.Infrastructure;

/// <summary>
/// Middleware that checks whether an authenticated Student or Marshal is still active.
/// If the user has been deactivated by an admin, returns 401 so the client clears
/// the session and redirects to login.
/// Admins are not checked (they manage the system and have no IsActive toggle).
/// </summary>
public class ActiveUserMiddleware
{
    private readonly RequestDelegate _next;

    public ActiveUserMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ShuttleDbContext db)
    {
        var user = context.User;

        // Only check authenticated requests with a role claim
        if (user.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        var role = user.FindFirstValue(ClaimTypes.Role);
        var subClaim = user.FindFirstValue(JwtRegisteredClaimNames.Sub)
                    ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(subClaim) || !int.TryParse(subClaim, out var userId))
        {
            await _next(context);
            return;
        }

        bool isActive = true;

        if (role == "Student")
        {
            var student = await db.Students
                .AsNoTracking()
                .Where(s => s.StudentId == userId)
                .Select(s => new { s.IsActive })
                .FirstOrDefaultAsync();

            isActive = student?.IsActive ?? false;
        }
        else if (role == "Marshal")
        {
            var marshal = await db.Marshals
                .AsNoTracking()
                .Where(m => m.MarshalId == userId)
                .Select(m => new { m.IsActive })
                .FirstOrDefaultAsync();

            isActive = marshal?.IsActive ?? false;
        }
        else
        {
            // Admin or unknown role — skip check
            await _next(context);
            return;
        }

        if (!isActive)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"message\":\"Your account has been deactivated. Please contact administration.\"}");
            return;
        }

        await _next(context);
    }
}
