namespace NmuShuttle.Api.DTOs;

public class TripDetailDto
{
    public int TripInstanceId { get; set; }
    public TimeOnly DepartureTime { get; set; }
    public string Direction { get; set; } = string.Empty;
    public string StopName { get; set; } = string.Empty;
    public string CampusName { get; set; } = string.Empty;
    public DateOnly TripDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int TotalCapacity { get; set; }
    public int ConfirmedCount { get; set; }
    public int AvailableSeats { get; set; }
    public DateTime? LockedAt { get; set; }
    public DateTime? InTransitAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<TripAllocationDto> Allocations { get; set; } = new();
}

public class TripAllocationDto
{
    public int AllocationId { get; set; }
    public int ShuttleId { get; set; }
    public string LicensePlate { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public bool IsAccessibleVehicle { get; set; }
    public int OrderPosition { get; set; }
    public bool IsOperational { get; set; }
}
