using NmuShuttle.Api.DTOs;

namespace NmuShuttle.Api.Services;

public interface ICsvUserImportService
{
    Task<CsvImportResultDto> ImportAsync(Stream csvStream);
}
