namespace NmuShuttle.Api.Infrastructure;

/// <summary>
/// Configures how the marshal scan window is enforced.
/// Bound from the "ScanWindow" configuration section.
/// </summary>
public class ScanWindowOptions
{
    public const string SectionName = "ScanWindow";

    /// <summary>
    /// Testing convenience. When true, every one of today's trips whose departure has
    /// not yet passed is open to scanning regardless of the 35-minute pre-departure
    /// window. Trips from tomorrow onward still follow the normal rule (only the next
    /// upcoming trip is open early; everything else opens at T-35 min).
    ///
    /// Set to false in production to enforce the strict 35-minute window for all trips.
    /// </summary>
    public bool OpenAllTodayTrips { get; set; }
}
