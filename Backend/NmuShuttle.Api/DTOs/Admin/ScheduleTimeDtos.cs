namespace NmuShuttle.Api.DTOs.Admin;

public class CreateScheduleTimeDto
{
    public TimeOnly DepartureTime { get; set; }
    public string Direction { get; set; } = string.Empty;
    public string ScheduleType { get; set; } = string.Empty;
    /// <summary>Ordered list of campus IDs (first = StopOrder 1).</summary>
    public List<int>? CampusIds { get; set; }
    public int StopId { get; set; }
    public bool RequiresScanning { get; set; } = true;
}

public class UpdateScheduleTimeDto
{
    public TimeOnly DepartureTime { get; set; }
    public string Direction { get; set; } = string.Empty;
    public string ScheduleType { get; set; } = string.Empty;
    /// <summary>Ordered list of campus IDs (first = StopOrder 1).</summary>
    public List<int>? CampusIds { get; set; }
    public int StopId { get; set; }
    public bool RequiresScanning { get; set; } = true;
}

public class ScheduleTimeResponseDto
{
    public int ScheduleTimeId { get; set; }
    public TimeOnly DepartureTime { get; set; }
    public string Direction { get; set; } = string.Empty;
    public string ScheduleType { get; set; } = string.Empty;
    public int StopId { get; set; }
    public bool RequiresScanning { get; set; }
    public List<ScheduleTimeCampusItemDto> Campuses { get; set; } = new();
}

public class ScheduleTimeCampusItemDto
{
    public int CampusId { get; set; }
    public string CampusName { get; set; } = string.Empty;
    public int StopOrder { get; set; }
}
