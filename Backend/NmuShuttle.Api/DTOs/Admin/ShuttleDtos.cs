namespace NmuShuttle.Api.DTOs.Admin;

public class CreateShuttleDto
{
    public string LicensePlate { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int VendorId { get; set; }
    public bool IsAccessibleVehicle { get; set; }
}

public class UpdateShuttleDto
{
    public string LicensePlate { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int VendorId { get; set; }
    public bool IsAccessibleVehicle { get; set; }
}

public class ShuttleResponseDto
{
    public int ShuttleId { get; set; }
    public string LicensePlate { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int VendorId { get; set; }
    public string VendorCompanyName { get; set; } = string.Empty;
    public bool IsAccessibleVehicle { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
