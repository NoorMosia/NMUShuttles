using NmuShuttle.Api.Models;

namespace NmuShuttle.Api.Services;

public interface ICheckInService
{
    Task<Booking> CheckInAsync(string qrCodeToken, int marshalId);

    /// <summary>
    /// Manual check-in by booking ID (dead-phone fallback). Same logic as QR scan
    /// but identifies the booking by ID instead of token.
    /// </summary>
    Task<Booking> ManualCheckInAsync(int bookingId, int marshalId);
}
