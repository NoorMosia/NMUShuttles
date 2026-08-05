namespace NmuShuttle.Api.DTOs.Admin;

public class CreateStopDto
{
    public string StopName { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string Suburb { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string GpsCoordinates { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class UpdateStopDto
{
    public string StopName { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string Suburb { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string GpsCoordinates { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class StopResponseDto
{
    public int StopId { get; set; }
    public string StopName { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string Suburb { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string FullAddress { get; set; } = string.Empty;
    public string GpsCoordinates { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
