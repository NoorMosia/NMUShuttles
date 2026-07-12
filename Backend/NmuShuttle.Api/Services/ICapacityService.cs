namespace NmuShuttle.Api.Services;

/// <summary>
/// Calculates seat capacity for a trip instance based on operational shuttle allocations.
/// </summary>
public interface ICapacityService
{
    /// <summary>
    /// Returns the total seat capacity (SUM of Shuttle.Capacity) for all operational allocations on a trip.
    /// </summary>
    Task<int> GetTotalCapacityAsync(int tripInstanceId);

    /// <summary>
    /// Returns the count of bookings with BookingStatus == "Confirmed" for the trip.
    /// </summary>
    Task<int> GetConfirmedCountAsync(int tripInstanceId);

    /// <summary>
    /// Returns TotalCapacity - ConfirmedCount (minimum 0).
    /// </summary>
    Task<int> GetAvailableSeatsAsync(int tripInstanceId);
}
