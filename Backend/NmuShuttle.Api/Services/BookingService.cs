using Microsoft.EntityFrameworkCore;
using NmuShuttle.Api.Data;
using NmuShuttle.Api.Models;

namespace NmuShuttle.Api.Services;

/// <summary>
/// Manages student bookings with capacity enforcement, waitlist logic,
/// and unique QR code token generation.
/// </summary>
public class BookingService : IBookingService
{
    private readonly ShuttleDbContext _db;
    private readonly ICapacityService _capacityService;

    public BookingService(ShuttleDbContext db, ICapacityService capacityService)
    {
        _db = db;
        _capacityService = capacityService;
    }

    /// <inheritdoc />
    public async Task<Booking> CreateBookingAsync(int studentId, int tripInstanceId)
    {
        // Look up trip instance
        var tripInstance = await _db.TripInstances.FindAsync(tripInstanceId);
        if (tripInstance == null)
            throw new KeyNotFoundException($"TripInstance with ID {tripInstanceId} not found.");

        // Only allow bookings on trips in Scheduling status
        if (tripInstance.Status != "Scheduling")
            throw new ArgumentException("Cannot book on a locked or completed trip");

        // Check for duplicate active booking (same student + trip, not cancelled)
        var duplicateExists = await _db.Bookings.AnyAsync(b =>
            b.StudentId == studentId &&
            b.TripInstanceId == tripInstanceId &&
            b.BookingStatus != "Cancelled");

        if (duplicateExists)
            throw new InvalidOperationException("Duplicate booking");

        // Determine booking status based on capacity
        var totalCapacity = await _capacityService.GetTotalCapacityAsync(tripInstanceId);
        var confirmedCount = await _capacityService.GetConfirmedCountAsync(tripInstanceId);

        var bookingStatus = confirmedCount < totalCapacity ? "Confirmed" : "WaitingList";

        // Generate unique QR code token with retry (up to 3 attempts)
        var booking = new Booking
        {
            StudentId = studentId,
            TripInstanceId = tripInstanceId,
            BookingStatus = bookingStatus,
            BookingType = "Standard",
            BoardedStatus = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        const int maxRetries = 3;
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            booking.QrCodeToken = Guid.NewGuid().ToString("N");

            _db.Bookings.Add(booking);

            try
            {
                await _db.SaveChangesAsync();
                return booking;
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex) && attempt < maxRetries)
            {
                // Detach and retry with a new token
                _db.Entry(booking).State = EntityState.Detached;
            }
        }

        // Final attempt already threw if it failed
        return booking;
    }

    /// <inheritdoc />
    public async Task<Booking> CancelBookingAsync(int bookingId, int studentId)
    {
        // Look up booking ensuring the student owns it
        var booking = await _db.Bookings.FirstOrDefaultAsync(b =>
            b.BookingId == bookingId && b.StudentId == studentId);

        if (booking == null)
            throw new KeyNotFoundException($"Booking with ID {bookingId} not found.");

        // Check trip status — cancellation only allowed during Scheduling
        var tripInstance = await _db.TripInstances.FindAsync(booking.TripInstanceId);
        if (tripInstance == null || tripInstance.Status != "Scheduling")
            throw new ArgumentException("Cannot cancel booking on a locked trip");

        // Cannot cancel an already-cancelled booking
        if (booking.BookingStatus == "Cancelled")
            throw new InvalidOperationException("Booking is already cancelled");

        booking.BookingStatus = "Cancelled";
        await _db.SaveChangesAsync();

        return booking;
    }

    /// <inheritdoc />
    public async Task<List<Booking>> GetStudentBookingsAsync(int studentId)
    {
        return await _db.Bookings
            .Where(b => b.StudentId == studentId)
            .Include(b => b.TripInstance)
                .ThenInclude(t => t.ScheduleTime)
            .Include(b => b.TripInstance)
                .ThenInclude(t => t.Stop)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Booking?> GetBookingByIdAsync(int bookingId, int studentId)
    {
        return await _db.Bookings
            .Where(b => b.BookingId == bookingId && b.StudentId == studentId)
            .Include(b => b.TripInstance)
                .ThenInclude(t => t.ScheduleTime)
            .Include(b => b.TripInstance)
                .ThenInclude(t => t.Stop)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Checks if a DbUpdateException is caused by a unique constraint violation.
    /// SQL Server error 2601 (unique index violation) or 2627 (unique constraint violation).
    /// </summary>
    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        var innerMessage = ex.InnerException?.Message ?? string.Empty;
        return innerMessage.Contains("2601") || innerMessage.Contains("2627");
    }
}
