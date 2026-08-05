namespace NmuShuttle.Api.DTOs.Admin;

public class CreateUserDto
{
    public string Type { get; set; } = string.Empty; // Student, Marshal, Admin
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // Student-specific
    public string? StudentNumber { get; set; }
    public bool? IsPriorityUser { get; set; }

    // Marshal-specific
    public string? StaffNumber { get; set; }
    public int? AssignedCampusId { get; set; }
    public int? AssignedStopId { get; set; }

    // Admin-specific
    public string? Password { get; set; }
}

public class UpdateUserDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // Student-specific
    public string? StudentNumber { get; set; }
    public bool? IsPriorityUser { get; set; }

    // Marshal-specific
    public string? StaffNumber { get; set; }
    public int? AssignedCampusId { get; set; }
    public int? AssignedStopId { get; set; }

    // Admin-specific
    public string? Password { get; set; }

    // Shared — toggle active/inactive
    public bool? IsActive { get; set; }
}

public class UserResponseDto
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Identifier { get; set; } // StudentNumber or StaffNumber
    public bool? IsPriorityUser { get; set; }
    public int? AssignedCampusId { get; set; }
    public int? AssignedStopId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}
