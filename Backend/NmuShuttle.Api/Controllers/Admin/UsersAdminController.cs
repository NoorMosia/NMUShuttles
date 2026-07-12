using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NmuShuttle.Api.Data;
using NmuShuttle.Api.DTOs;
using NmuShuttle.Api.DTOs.Admin;
using NmuShuttle.Api.Models;
using NmuShuttle.Api.Services;

namespace NmuShuttle.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/users")]
[Authorize(Roles = "Admin")]
public class UsersAdminController : ControllerBase
{
    private readonly ShuttleDbContext _context;
    private readonly ICsvUserImportService _csvUserImportService;

    public UsersAdminController(ShuttleDbContext context, ICsvUserImportService csvUserImportService)
    {
        _context = context;
        _csvUserImportService = csvUserImportService;
    }

    /// <summary>
    /// Bulk import users from a CSV file.
    /// CSV format: Role,Identifier,FirstName,LastName,Email (with header row).
    /// Skips invalid/duplicate rows and returns a summary.
    /// </summary>
    [HttpPost("bulk")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB max
    public async Task<IActionResult> BulkImport(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "A file is required" });

        // Validate file extension
        var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
        if (extension != ".csv")
            return BadRequest(new { message = "Only .csv files are accepted" });

        using var stream = file.OpenReadStream();
        var result = await _csvUserImportService.ImportAsync(stream);

        return Ok(result);
    }

    /// <summary>
    /// List all users (Students + Marshals + Admins) with optional role filter and search.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? role, [FromQuery] string? search)
    {
        var users = new List<UserResponseDto>();

        var includeStudents = string.IsNullOrEmpty(role) || role.Equals("Student", StringComparison.OrdinalIgnoreCase);
        var includeMarshals = string.IsNullOrEmpty(role) || role.Equals("Marshal", StringComparison.OrdinalIgnoreCase);
        var includeAdmins = string.IsNullOrEmpty(role) || role.Equals("Admin", StringComparison.OrdinalIgnoreCase);

        if (includeStudents)
        {
            var studentsQuery = _context.Students.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                studentsQuery = studentsQuery.Where(st =>
                    st.FirstName.ToLower().Contains(s) ||
                    st.LastName.ToLower().Contains(s) ||
                    st.StudentNumber.ToLower().Contains(s) ||
                    st.UniversityEmail.ToLower().Contains(s));
            }

            var students = await studentsQuery.Select(st => new UserResponseDto
            {
                Id = st.StudentId,
                Type = "Student",
                FirstName = st.FirstName,
                LastName = st.LastName,
                Email = st.UniversityEmail,
                Identifier = st.StudentNumber,
                IsPriorityUser = st.IsPriorityUser,
                CreatedAt = st.CreatedAt
            }).ToListAsync();

            users.AddRange(students);
        }

        if (includeMarshals)
        {
            var marshalsQuery = _context.Marshals.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                marshalsQuery = marshalsQuery.Where(m =>
                    m.FirstName.ToLower().Contains(s) ||
                    m.LastName.ToLower().Contains(s) ||
                    m.StaffNumber.ToLower().Contains(s) ||
                    m.UniversityEmail.ToLower().Contains(s));
            }

            var marshals = await marshalsQuery.Select(m => new UserResponseDto
            {
                Id = m.MarshalId,
                Type = "Marshal",
                FirstName = m.FirstName,
                LastName = m.LastName,
                Email = m.UniversityEmail,
                Identifier = m.StaffNumber,
                AssignedCampusId = m.AssignedCampusId,
                AssignedStopId = m.AssignedStopId,
                CreatedAt = m.CreatedAt
            }).ToListAsync();

            users.AddRange(marshals);
        }

        if (includeAdmins)
        {
            var adminsQuery = _context.Admins.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                adminsQuery = adminsQuery.Where(a =>
                    a.FirstName.ToLower().Contains(s) ||
                    a.LastName.ToLower().Contains(s) ||
                    a.Email.ToLower().Contains(s));
            }

            var admins = await adminsQuery.Select(a => new UserResponseDto
            {
                Id = a.AdminId,
                Type = "Admin",
                FirstName = a.FirstName,
                LastName = a.LastName,
                Email = a.Email,
                CreatedAt = a.CreatedAt
            }).ToListAsync();

            users.AddRange(admins);
        }

        return Ok(users);
    }

    /// <summary>
    /// Get a single user by ID and type.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, [FromQuery] string type)
    {
        if (string.IsNullOrWhiteSpace(type))
            return BadRequest(new { message = "Query parameter 'type' is required (Student|Marshal|Admin)" });

        switch (type.ToLower())
        {
            case "student":
                var student = await _context.Students.FindAsync(id);
                if (student == null) return NotFound(new { message = "Student not found" });
                return Ok(new UserResponseDto
                {
                    Id = student.StudentId,
                    Type = "Student",
                    FirstName = student.FirstName,
                    LastName = student.LastName,
                    Email = student.UniversityEmail,
                    Identifier = student.StudentNumber,
                    IsPriorityUser = student.IsPriorityUser,
                    CreatedAt = student.CreatedAt
                });

            case "marshal":
                var marshal = await _context.Marshals.FindAsync(id);
                if (marshal == null) return NotFound(new { message = "Marshal not found" });
                return Ok(new UserResponseDto
                {
                    Id = marshal.MarshalId,
                    Type = "Marshal",
                    FirstName = marshal.FirstName,
                    LastName = marshal.LastName,
                    Email = marshal.UniversityEmail,
                    Identifier = marshal.StaffNumber,
                    AssignedCampusId = marshal.AssignedCampusId,
                    AssignedStopId = marshal.AssignedStopId,
                    CreatedAt = marshal.CreatedAt
                });

            case "admin":
                var admin = await _context.Admins.FindAsync(id);
                if (admin == null) return NotFound(new { message = "Admin not found" });
                return Ok(new UserResponseDto
                {
                    Id = admin.AdminId,
                    Type = "Admin",
                    FirstName = admin.FirstName,
                    LastName = admin.LastName,
                    Email = admin.Email,
                    CreatedAt = admin.CreatedAt
                });

            default:
                return BadRequest(new { message = "Invalid type. Must be Student, Marshal, or Admin" });
        }
    }

    /// <summary>
    /// Create a new user. Validates StudentNumber/StaffNumber format; rejects duplicate email/identifier with 409.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Type))
            return BadRequest(new { message = "Type is required (Student|Marshal|Admin)" });

        switch (dto.Type.ToLower())
        {
            case "student":
                return await CreateStudent(dto);
            case "marshal":
                return await CreateMarshal(dto);
            case "admin":
                return await CreateAdmin(dto);
            default:
                return BadRequest(new { message = "Invalid type. Must be Student, Marshal, or Admin" });
        }
    }

    /// <summary>
    /// Update an existing user by ID and type.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromQuery] string type, [FromBody] UpdateUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(type))
            return BadRequest(new { message = "Query parameter 'type' is required (Student|Marshal|Admin)" });

        switch (type.ToLower())
        {
            case "student":
                return await UpdateStudent(id, dto);
            case "marshal":
                return await UpdateMarshal(id, dto);
            case "admin":
                return await UpdateAdmin(id, dto);
            default:
                return BadRequest(new { message = "Invalid type. Must be Student, Marshal, or Admin" });
        }
    }

    private async Task<IActionResult> CreateStudent(CreateUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.StudentNumber))
            return UnprocessableEntity(new { message = "StudentNumber is required for student users" });

        // Validate StudentNumber format: s3[12]\d{7}
        if (!Regex.IsMatch(dto.StudentNumber, @"^s3[12]\d{7}$"))
            return UnprocessableEntity(new { message = "StudentNumber must match format: s3[1 or 2] followed by 7 digits (e.g., s312345678)" });

        // Reject duplicate email
        var emailDuplicate = await _context.Students
            .AnyAsync(s => s.UniversityEmail.ToLower() == dto.Email.ToLower());
        if (emailDuplicate)
            return Conflict(new { message = "A student with this email already exists" });

        // Reject duplicate StudentNumber
        var numberDuplicate = await _context.Students
            .AnyAsync(s => s.StudentNumber.ToLower() == dto.StudentNumber.ToLower());
        if (numberDuplicate)
            return Conflict(new { message = "A student with this student number already exists" });

        var student = new Student
        {
            StudentNumber = dto.StudentNumber,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            UniversityEmail = dto.Email,
            IsPriorityUser = dto.IsPriorityUser ?? false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Students.Add(student);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = student.StudentId, type = "Student" }, new UserResponseDto
        {
            Id = student.StudentId,
            Type = "Student",
            FirstName = student.FirstName,
            LastName = student.LastName,
            Email = student.UniversityEmail,
            Identifier = student.StudentNumber,
            IsPriorityUser = student.IsPriorityUser,
            CreatedAt = student.CreatedAt
        });
    }

    private async Task<IActionResult> CreateMarshal(CreateUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.StaffNumber))
            return UnprocessableEntity(new { message = "StaffNumber is required for marshal users" });

        // Validate StaffNumber format: 7 digits
        if (!Regex.IsMatch(dto.StaffNumber, @"^\d{7}$"))
            return UnprocessableEntity(new { message = "StaffNumber must be exactly 7 digits" });

        // Reject duplicate email
        var emailDuplicate = await _context.Marshals
            .AnyAsync(m => m.UniversityEmail.ToLower() == dto.Email.ToLower());
        if (emailDuplicate)
            return Conflict(new { message = "A marshal with this email already exists" });

        // Reject duplicate StaffNumber
        var numberDuplicate = await _context.Marshals
            .AnyAsync(m => m.StaffNumber == dto.StaffNumber);
        if (numberDuplicate)
            return Conflict(new { message = "A marshal with this staff number already exists" });

        var marshal = new Marshal
        {
            StaffNumber = dto.StaffNumber,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            UniversityEmail = dto.Email,
            AssignedCampusId = dto.AssignedCampusId,
            AssignedStopId = dto.AssignedStopId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Marshals.Add(marshal);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = marshal.MarshalId, type = "Marshal" }, new UserResponseDto
        {
            Id = marshal.MarshalId,
            Type = "Marshal",
            FirstName = marshal.FirstName,
            LastName = marshal.LastName,
            Email = marshal.UniversityEmail,
            Identifier = marshal.StaffNumber,
            AssignedCampusId = marshal.AssignedCampusId,
            AssignedStopId = marshal.AssignedStopId,
            CreatedAt = marshal.CreatedAt
        });
    }

    private async Task<IActionResult> CreateAdmin(CreateUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Password))
            return UnprocessableEntity(new { message = "Password is required for admin users" });

        // Reject duplicate email
        var emailDuplicate = await _context.Admins
            .AnyAsync(a => a.Email.ToLower() == dto.Email.ToLower());
        if (emailDuplicate)
            return Conflict(new { message = "An admin with this email already exists" });

        var admin = new Models.Admin
        {
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, workFactor: 12),
            CreatedAt = DateTime.UtcNow
        };

        _context.Admins.Add(admin);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = admin.AdminId, type = "Admin" }, new UserResponseDto
        {
            Id = admin.AdminId,
            Type = "Admin",
            FirstName = admin.FirstName,
            LastName = admin.LastName,
            Email = admin.Email,
            CreatedAt = admin.CreatedAt
        });
    }

    private async Task<IActionResult> UpdateStudent(int id, UpdateUserDto dto)
    {
        var student = await _context.Students.FindAsync(id);
        if (student == null)
            return NotFound(new { message = "Student not found" });

        if (!string.IsNullOrWhiteSpace(dto.StudentNumber) &&
            !Regex.IsMatch(dto.StudentNumber, @"^s3[12]\d{7}$"))
            return UnprocessableEntity(new { message = "StudentNumber must match format: s3[1 or 2] followed by 7 digits" });

        // Reject duplicate email (exclude current)
        var emailDuplicate = await _context.Students
            .AnyAsync(s => s.UniversityEmail.ToLower() == dto.Email.ToLower() && s.StudentId != id);
        if (emailDuplicate)
            return Conflict(new { message = "A student with this email already exists" });

        if (!string.IsNullOrWhiteSpace(dto.StudentNumber))
        {
            var numberDuplicate = await _context.Students
                .AnyAsync(s => s.StudentNumber.ToLower() == dto.StudentNumber.ToLower() && s.StudentId != id);
            if (numberDuplicate)
                return Conflict(new { message = "A student with this student number already exists" });
            student.StudentNumber = dto.StudentNumber;
        }

        student.FirstName = dto.FirstName;
        student.LastName = dto.LastName;
        student.UniversityEmail = dto.Email;
        if (dto.IsPriorityUser.HasValue)
            student.IsPriorityUser = dto.IsPriorityUser.Value;

        await _context.SaveChangesAsync();

        return Ok(new UserResponseDto
        {
            Id = student.StudentId,
            Type = "Student",
            FirstName = student.FirstName,
            LastName = student.LastName,
            Email = student.UniversityEmail,
            Identifier = student.StudentNumber,
            IsPriorityUser = student.IsPriorityUser,
            CreatedAt = student.CreatedAt
        });
    }

    private async Task<IActionResult> UpdateMarshal(int id, UpdateUserDto dto)
    {
        var marshal = await _context.Marshals.FindAsync(id);
        if (marshal == null)
            return NotFound(new { message = "Marshal not found" });

        if (!string.IsNullOrWhiteSpace(dto.StaffNumber) &&
            !Regex.IsMatch(dto.StaffNumber, @"^\d{7}$"))
            return UnprocessableEntity(new { message = "StaffNumber must be exactly 7 digits" });

        // Reject duplicate email (exclude current)
        var emailDuplicate = await _context.Marshals
            .AnyAsync(m => m.UniversityEmail.ToLower() == dto.Email.ToLower() && m.MarshalId != id);
        if (emailDuplicate)
            return Conflict(new { message = "A marshal with this email already exists" });

        if (!string.IsNullOrWhiteSpace(dto.StaffNumber))
        {
            var numberDuplicate = await _context.Marshals
                .AnyAsync(m => m.StaffNumber == dto.StaffNumber && m.MarshalId != id);
            if (numberDuplicate)
                return Conflict(new { message = "A marshal with this staff number already exists" });
            marshal.StaffNumber = dto.StaffNumber;
        }

        marshal.FirstName = dto.FirstName;
        marshal.LastName = dto.LastName;
        marshal.UniversityEmail = dto.Email;
        marshal.AssignedCampusId = dto.AssignedCampusId;
        marshal.AssignedStopId = dto.AssignedStopId;

        await _context.SaveChangesAsync();

        return Ok(new UserResponseDto
        {
            Id = marshal.MarshalId,
            Type = "Marshal",
            FirstName = marshal.FirstName,
            LastName = marshal.LastName,
            Email = marshal.UniversityEmail,
            Identifier = marshal.StaffNumber,
            AssignedCampusId = marshal.AssignedCampusId,
            AssignedStopId = marshal.AssignedStopId,
            CreatedAt = marshal.CreatedAt
        });
    }

    private async Task<IActionResult> UpdateAdmin(int id, UpdateUserDto dto)
    {
        var admin = await _context.Admins.FindAsync(id);
        if (admin == null)
            return NotFound(new { message = "Admin not found" });

        // Reject duplicate email (exclude current)
        var emailDuplicate = await _context.Admins
            .AnyAsync(a => a.Email.ToLower() == dto.Email.ToLower() && a.AdminId != id);
        if (emailDuplicate)
            return Conflict(new { message = "An admin with this email already exists" });

        admin.FirstName = dto.FirstName;
        admin.LastName = dto.LastName;
        admin.Email = dto.Email;

        if (!string.IsNullOrWhiteSpace(dto.Password))
            admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, workFactor: 12);

        await _context.SaveChangesAsync();

        return Ok(new UserResponseDto
        {
            Id = admin.AdminId,
            Type = "Admin",
            FirstName = admin.FirstName,
            LastName = admin.LastName,
            Email = admin.Email,
            CreatedAt = admin.CreatedAt
        });
    }
}
