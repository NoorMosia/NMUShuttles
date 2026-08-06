namespace NmuShuttle.Api.DTOs.Admin;

public class AddToDefaultConvoyDto
{
    public int ShuttleId { get; set; }
}

public class ReorderDefaultConvoyDto
{
    public int[] OrderedShuttleIds { get; set; } = Array.Empty<int>();
}
