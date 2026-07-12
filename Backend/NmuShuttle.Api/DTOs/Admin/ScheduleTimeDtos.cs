namespace NmuShuttle.Api.DTOs.Admin;

public class CreateScheduleTimeDto
{
    public TimeOnly DepartureTime { get; set; }
    public string Direction { get; set; } = string.Empty;
    public string ScheduleType { get; set; } = string.Empty;
    public int CampusId { get; set; }
    public int StopId { get; set; }
}

public class UpdateScheduleTimeDto
{
    public TimeOnly DepartureTime { get; set; }
    public string Direction { get; set; } = string.Empty;
    public string ScheduleType { get; set; } = string.Empty;
    public int CampusId { get; set; }
    public int StopId { get; set; }
}

public class ScheduleTimeResponseDto
{
    public int ScheduleTimeId { get; set; }
    public TimeOnly DepartureTime { get; set; }
    public string Direction { get; set; } = string.Empty;
    public string ScheduleType { get; set; } = string.Empty;
    public int CampusId { get; set; }
    public int StopId { get; set; }
}
