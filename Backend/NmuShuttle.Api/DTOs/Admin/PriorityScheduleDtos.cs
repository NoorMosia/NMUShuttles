namespace NmuShuttle.Api.DTOs.Admin;

public class CreatePriorityScheduleDto
{
    public int StudentId { get; set; }
    public int StopId { get; set; }
    public int ScheduleTimeId { get; set; }
    public string DayOfWeek { get; set; } = string.Empty;
}

public class PriorityScheduleResponseDto
{
    public int PriorityScheduleId { get; set; }
    public int StudentId { get; set; }
    public int StopId { get; set; }
    public int ScheduleTimeId { get; set; }
    public string DayOfWeek { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
