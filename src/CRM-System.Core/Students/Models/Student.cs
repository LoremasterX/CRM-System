namespace CRM_System.Core.Students.Models;

public class Student
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string StudentNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string University { get; set; } = string.Empty;
    public string Faculty { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string StudyProgram { get; set; } = string.Empty;
    public int StudyYear { get; set; } = 1;
    public string? Bio { get; set; }
    public StudentStatus Status { get; set; } = StudentStatus.Active;
    public DateOnly EnrollmentDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser? User { get; set; }

    public bool IsActive => Status == StudentStatus.Active;

    public string Initials
    {
        get
        {
            if (string.IsNullOrWhiteSpace(FullName))
            {
                return "ST";
            }

            var parts = FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return parts.Length switch
            {
                0 => "ST",
                1 => parts[0][..Math.Min(2, parts[0].Length)].ToUpperInvariant(),
                _ => $"{char.ToUpperInvariant(parts[0][0])}{char.ToUpperInvariant(parts[^1][0])}"
            };
        }
    }
}
