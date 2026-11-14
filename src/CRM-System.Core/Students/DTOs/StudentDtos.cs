using System.ComponentModel.DataAnnotations;
using CRM_System.Core.Students.Models;

namespace CRM_System.Core.Students.DTOs;

public sealed record StudentDto(
    int Id,
    int? UserId,
    string StudentNumber,
    string FullName,
    string Email,
    string? PhoneNumber,
    DateOnly? DateOfBirth,
    string University,
    string Faculty,
    string Department,
    string StudyProgram,
    int StudyYear,
    string? Bio,
    StudentStatus Status,
    DateOnly EnrollmentDate,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string Initials)
{
    public bool IsActive => Status == StudentStatus.Active;

    public static StudentDto FromEntity(Student student) => new(
        student.Id,
        student.UserId,
        student.StudentNumber,
        student.FullName,
        student.Email,
        student.PhoneNumber,
        student.DateOfBirth,
        student.University,
        student.Faculty,
        student.Department,
        student.StudyProgram,
        student.StudyYear,
        student.Bio,
        student.Status,
        student.EnrollmentDate,
        student.CreatedAt,
        student.UpdatedAt,
        student.Initials);
}

public class CreateStudentRequestDto
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 120 characters.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [StringLength(160)]
    public string Email { get; set; } = string.Empty;

    [StringLength(32, ErrorMessage = "Student ID cannot exceed 32 characters.")]
    public string? StudentNumber { get; set; }

    [StringLength(32)]
    public string? PhoneNumber { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    [Required(ErrorMessage = "University is required.")]
    [StringLength(150, MinimumLength = 2)]
    public string University { get; set; } = "Astana IT University";

    [Required(ErrorMessage = "Faculty is required.")]
    [StringLength(120, MinimumLength = 2)]
    public string Faculty { get; set; } = "School of Software Engineering";

    [Required(ErrorMessage = "Department is required.")]
    [StringLength(120, MinimumLength = 2)]
    public string Department { get; set; } = "Computer Science & Engineering";

    [Required(ErrorMessage = "Study program is required.")]
    [StringLength(120, MinimumLength = 2)]
    public string StudyProgram { get; set; } = "B.Sc. in Software Engineering";

    [Range(1, 6, ErrorMessage = "Study year must be between 1 and 6.")]
    public int StudyYear { get; set; } = 1;

    [StringLength(1000)]
    public string? Bio { get; set; }

    public StudentStatus Status { get; set; } = StudentStatus.Active;

    public DateOnly? EnrollmentDate { get; set; }

    public bool CreateLoginAccount { get; set; } = true;

    [StringLength(100)]
    public string? InitialPassword { get; set; } = "Student123!";
}

public class UpdateStudentRequestDto
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 120 characters.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [StringLength(160)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Student ID is required.")]
    [StringLength(32, MinimumLength = 3)]
    public string StudentNumber { get; set; } = string.Empty;

    [StringLength(32)]
    public string? PhoneNumber { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    [Required(ErrorMessage = "University is required.")]
    [StringLength(150, MinimumLength = 2)]
    public string University { get; set; } = string.Empty;

    [Required(ErrorMessage = "Faculty is required.")]
    [StringLength(120, MinimumLength = 2)]
    public string Faculty { get; set; } = string.Empty;

    [Required(ErrorMessage = "Department is required.")]
    [StringLength(120, MinimumLength = 2)]
    public string Department { get; set; } = string.Empty;

    [Required(ErrorMessage = "Study program is required.")]
    [StringLength(120, MinimumLength = 2)]
    public string StudyProgram { get; set; } = string.Empty;

    [Range(1, 6, ErrorMessage = "Study year must be between 1 and 6.")]
    public int StudyYear { get; set; } = 1;

    [StringLength(1000)]
    public string? Bio { get; set; }

    public StudentStatus Status { get; set; } = StudentStatus.Active;

    public DateOnly EnrollmentDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
}

public class UpdatePersonalProfileRequestDto
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 120 characters.")]
    public string FullName { get; set; } = string.Empty;

    [StringLength(32)]
    public string? PhoneNumber { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    [Required(ErrorMessage = "University is required.")]
    [StringLength(150, MinimumLength = 2)]
    public string University { get; set; } = string.Empty;

    [Required(ErrorMessage = "Faculty is required.")]
    [StringLength(120, MinimumLength = 2)]
    public string Faculty { get; set; } = string.Empty;

    [Required(ErrorMessage = "Department is required.")]
    [StringLength(120, MinimumLength = 2)]
    public string Department { get; set; } = string.Empty;

    [Required(ErrorMessage = "Study program is required.")]
    [StringLength(120, MinimumLength = 2)]
    public string StudyProgram { get; set; } = string.Empty;

    [Range(1, 6, ErrorMessage = "Study year must be between 1 and 6.")]
    public int StudyYear { get; set; } = 1;

    [StringLength(1000)]
    public string? Bio { get; set; }
}

public class StudentFilterDto
{
    public string? SearchTerm { get; set; }
    public string? Faculty { get; set; }
    public int? StudyYear { get; set; }
    public StudentStatus? Status { get; set; }
    public string SortBy { get; set; } = "FullName";
    public bool SortDescending { get; set; }
}

public sealed record StudentDirectoryStatsDto(
    int TotalStudents,
    int ActiveStudents,
    int InactiveStudents,
    int GraduatedStudents,
    int SuspendedStudents,
    int FacultiesCount,
    int DepartmentsCount);
