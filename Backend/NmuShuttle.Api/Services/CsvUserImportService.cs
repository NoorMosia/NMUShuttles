using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using NmuShuttle.Api.Data;
using NmuShuttle.Api.DTOs;
using NmuShuttle.Api.Models;

namespace NmuShuttle.Api.Services;

public class CsvUserImportService : ICsvUserImportService
{
    private readonly ShuttleDbContext _context;

    private static readonly Regex StudentIdentifierRegex = new(@"^s3[12]\d{7}$", RegexOptions.Compiled);
    private static readonly Regex MarshalIdentifierRegex = new(@"^\d{7}$", RegexOptions.Compiled);

    public CsvUserImportService(ShuttleDbContext context)
    {
        _context = context;
    }

    public async Task<CsvImportResultDto> ImportAsync(Stream csvStream)
    {
        var result = new CsvImportResultDto();
        var seenEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var seenIdentifiers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        using var reader = new StreamReader(csvStream);

        // Read and validate header row
        var headerLine = await reader.ReadLineAsync();
        if (string.IsNullOrWhiteSpace(headerLine))
        {
            result.Skipped.Add(new CsvSkipErrorDto { Row = 1, Reason = "Empty file or missing header row" });
            return result;
        }

        var rowNumber = 1; // Header is row 1, data starts at row 2

        while (!reader.EndOfStream)
        {
            rowNumber++;
            var line = await reader.ReadLineAsync();

            if (string.IsNullOrWhiteSpace(line))
            {
                result.Skipped.Add(new CsvSkipErrorDto { Row = rowNumber, Reason = "Empty row" });
                continue;
            }

            var columns = ParseCsvLine(line);

            if (columns.Length < 5)
            {
                result.Skipped.Add(new CsvSkipErrorDto { Row = rowNumber, Reason = "Insufficient columns; expected Role,Identifier,FirstName,LastName,Email" });
                continue;
            }

            var role = columns[0].Trim();
            var identifier = columns[1].Trim();
            var firstName = columns[2].Trim();
            var lastName = columns[3].Trim();
            var email = columns[4].Trim();

            // Validate role
            if (!role.Equals("Student", StringComparison.OrdinalIgnoreCase) &&
                !role.Equals("Marshal", StringComparison.OrdinalIgnoreCase))
            {
                result.Skipped.Add(new CsvSkipErrorDto { Row = rowNumber, Reason = $"Invalid role '{role}'; must be Student or Marshal" });
                continue;
            }

            // Validate identifier format
            var isStudent = role.Equals("Student", StringComparison.OrdinalIgnoreCase);
            if (isStudent && !StudentIdentifierRegex.IsMatch(identifier))
            {
                result.Skipped.Add(new CsvSkipErrorDto { Row = rowNumber, Reason = $"Invalid student identifier format '{identifier}'; must match s3[1|2] followed by 7 digits" });
                continue;
            }

            if (!isStudent && !MarshalIdentifierRegex.IsMatch(identifier))
            {
                result.Skipped.Add(new CsvSkipErrorDto { Row = rowNumber, Reason = $"Invalid marshal identifier format '{identifier}'; must be exactly 7 digits" });
                continue;
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(firstName))
            {
                result.Skipped.Add(new CsvSkipErrorDto { Row = rowNumber, Reason = "FirstName is required" });
                continue;
            }

            if (string.IsNullOrWhiteSpace(lastName))
            {
                result.Skipped.Add(new CsvSkipErrorDto { Row = rowNumber, Reason = "LastName is required" });
                continue;
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                result.Skipped.Add(new CsvSkipErrorDto { Row = rowNumber, Reason = "Email is required" });
                continue;
            }

            // Check duplicate email within file
            if (seenEmails.Contains(email))
            {
                result.Skipped.Add(new CsvSkipErrorDto { Row = rowNumber, Reason = $"Duplicate email '{email}' within file" });
                continue;
            }

            // Check duplicate identifier within file
            if (seenIdentifiers.Contains(identifier))
            {
                result.Skipped.Add(new CsvSkipErrorDto { Row = rowNumber, Reason = $"Duplicate identifier '{identifier}' within file" });
                continue;
            }

            // Check duplicate email against DB (Students + Marshals)
            var emailExistsInDb = await _context.Students
                .AnyAsync(s => s.UniversityEmail.ToLower() == email.ToLower())
                || await _context.Marshals
                .AnyAsync(m => m.UniversityEmail.ToLower() == email.ToLower());

            if (emailExistsInDb)
            {
                result.Skipped.Add(new CsvSkipErrorDto { Row = rowNumber, Reason = $"Email '{email}' already exists in the database" });
                continue;
            }

            // Check duplicate identifier against DB
            bool identifierExistsInDb;
            if (isStudent)
            {
                identifierExistsInDb = await _context.Students
                    .AnyAsync(s => s.StudentNumber.ToLower() == identifier.ToLower());
            }
            else
            {
                identifierExistsInDb = await _context.Marshals
                    .AnyAsync(m => m.StaffNumber == identifier);
            }

            if (identifierExistsInDb)
            {
                result.Skipped.Add(new CsvSkipErrorDto { Row = rowNumber, Reason = $"Identifier '{identifier}' already exists in the database" });
                continue;
            }

            // All validations passed — create the record
            if (isStudent)
            {
                var student = new Student
                {
                    StudentNumber = identifier,
                    FirstName = firstName,
                    LastName = lastName,
                    UniversityEmail = email,
                    IsPriorityUser = false,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Students.Add(student);
            }
            else
            {
                var marshal = new Marshal
                {
                    StaffNumber = identifier,
                    FirstName = firstName,
                    LastName = lastName,
                    UniversityEmail = email,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Marshals.Add(marshal);
            }

            seenEmails.Add(email);
            seenIdentifiers.Add(identifier);
            result.Created++;
        }

        if (result.Created > 0)
        {
            await _context.SaveChangesAsync();
        }

        return result;
    }

    /// <summary>
    /// Simple CSV line parser that handles basic comma-separated values.
    /// Does not handle quoted fields with embedded commas for simplicity.
    /// </summary>
    private static string[] ParseCsvLine(string line)
    {
        return line.Split(',');
    }
}
