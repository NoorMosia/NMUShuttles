namespace NmuShuttle.Api.DTOs.Admin;

public class CreateCalendarExceptionDto
{
    public DateOnly TargetDate { get; set; }
    public string ExceptionType { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class CalendarExceptionResponseDto
{
    public int CalendarExceptionId { get; set; }
    public DateOnly TargetDate { get; set; }
    public string ExceptionType { get; set; } = string.Empty;
    public string? Description { get; set; }
}
