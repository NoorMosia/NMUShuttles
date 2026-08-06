namespace NmuShuttle.Api.DTOs.Admin;

public class AddCampusToRouteDto
{
    public int CampusId { get; set; }
    public int? StopOrder { get; set; }
}

public class ReorderCampusesDto
{
    public int[] OrderedCampusIds { get; set; } = Array.Empty<int>();
}
