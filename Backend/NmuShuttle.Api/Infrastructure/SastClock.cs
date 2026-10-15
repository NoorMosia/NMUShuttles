namespace NmuShuttle.Api.Infrastructure;

/// <summary>
/// Single source of truth for South African Standard Time (SAST) handling.
///
/// Convention for the whole backend:
///   • All instants are stored and compared in UTC.
///   • SAST is applied only at the edges where business logic needs a local
///     calendar day or wall-clock time (trip generation, lockout, scan window).
///
/// SAST is UTC+2 year-round — South Africa observes no daylight saving — so a
/// fixed offset is correct and avoids any dependency on the host OS having the
/// "South Africa Standard Time" zone installed.
///
/// Every <c>AddHours(2)</c> / <c>AddHours(-2)</c> / <c>SastOffset</c> magic number
/// that used to be scattered across the services and controllers now routes
/// through here.
/// </summary>
public static class SastClock
{
    /// <summary>SAST offset from UTC (UTC+2, no DST).</summary>
    public static readonly TimeSpan Offset = TimeSpan.FromHours(2);

    /// <summary>Current instant in UTC. Thin wrapper so call sites read intentfully.</summary>
    public static DateTime UtcNow => DateTime.UtcNow;

    /// <summary>The current SAST wall-clock time (UTC+2). Not an absolute instant — use for display/day math only.</summary>
    public static DateTime Now => DateTime.UtcNow + Offset;

    /// <summary>Today's date on the SAST calendar.</summary>
    public static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow + Offset);

    /// <summary>
    /// Converts a SAST calendar date + SAST wall-clock time into its UTC instant.
    /// Used to turn a trip's (TripDate, DepartureTime) pair into an absolute instant.
    /// </summary>
    public static DateTime ToUtc(DateOnly sastDate, TimeOnly sastWallClock)
        => sastDate.ToDateTime(sastWallClock) - Offset;

    /// <summary>Converts a UTC instant into the SAST calendar date it falls on.</summary>
    public static DateOnly ToSastDate(DateTime utc)
        => DateOnly.FromDateTime(utc + Offset);

    /// <summary>
    /// The UTC instant of the next SAST midnight (00:00 SAST tomorrow), given the
    /// current UTC instant. Used by the nightly rolling-injection scheduler.
    /// </summary>
    public static DateTime NextMidnightUtc(DateTime utcNow)
    {
        var sastNow = utcNow + Offset;
        var nextMidnightSast = sastNow.Date.AddDays(1); // 00:00 SAST tomorrow
        return nextMidnightSast - Offset;               // back to UTC
    }
}
