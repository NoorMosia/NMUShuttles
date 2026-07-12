namespace NmuShuttle.Api.DTOs;

public class NotificationResponseDto
{
    public int NotificationId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public int? RelatedTripInstanceId { get; set; }
    public DateTime CreatedAt { get; set; }
}
