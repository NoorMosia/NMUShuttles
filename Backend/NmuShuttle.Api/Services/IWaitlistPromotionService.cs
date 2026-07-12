namespace NmuShuttle.Api.Services;

/// <summary>
/// Promotes waitlisted bookings to confirmed status when capacity becomes available.
/// </summary>
public interface IWaitlistPromotionService
{
    /// <summary>
    /// Promotes WaitingList bookings in FIFO order (CreatedAt ASC) up to available capacity.
    /// </summary>
    Task RunAsync(int tripInstanceId);
}
