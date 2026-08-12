namespace NmuShuttle.Api.DTOs;

/// <summary>
/// Paginated response wrapper for student bookings.
/// </summary>
public class PaginatedBookingResponseDto
{
    public List<BookingResponseDto> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
