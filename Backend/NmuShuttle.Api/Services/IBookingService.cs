using NmuShuttle.Api.Models;

namespace NmuShuttle.Api.Services;

/// <summary>
/// Manages student bookings with capacity enforcement and waitlist logic.
/// </summary>
public interface IBookingService
{
    /// <summary>
    /// Creates a booking for a student on a trip instance.
    /// Assigns Confirmed or WaitingList status based on available capacity.
    /// </summary>
    Task<Booking> CreateBookingAsync(int studentId, int tripInstanceId);

    /// <summary>
    /// Cancels a booking. Only allowed while the trip is in Scheduling status.
    /// </summary>
    Task<Booking> CancelBookingAsync(int bookingId, int studentId);

    /// <summary>
    /// Returns all bookings for a student, ordered by CreatedAt descending.
    /// </summary>
    Task<List<Booking>> GetStudentBookingsAsync(int studentId);

    /// <summary>
    /// Returns a single booking by ID, ensuring the student owns it.
    /// </summary>
    Task<Booking?> GetBookingByIdAsync(int bookingId, int studentId);
}
