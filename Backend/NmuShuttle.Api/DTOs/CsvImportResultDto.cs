namespace NmuShuttle.Api.DTOs;

public class CsvImportResultDto
{
    public int Created { get; set; }
    public List<CsvSkipErrorDto> Skipped { get; set; } = new();
}

public class CsvSkipErrorDto
{
    public int Row { get; set; }
    public string Reason { get; set; } = string.Empty;
}
