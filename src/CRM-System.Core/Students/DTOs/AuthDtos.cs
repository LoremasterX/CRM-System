using System.ComponentModel.DataAnnotations;

namespace CRM_System.Core.Students.DTOs;

public class LoginRequestDto
{
    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; } = true;
}

public class RegisterStudentRequestDto
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 120 characters.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid university email address.")]
    [StringLength(160)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your password.")]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [StringLength(32)]
    public string? StudentNumber { get; set; }

    [Required(ErrorMessage = "University name is required.")]
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

    [StringLength(32)]
    public string? PhoneNumber { get; set; }

    [StringLength(1000)]
    public string? Bio { get; set; }
}

public class ChangePasswordRequestDto
{
    [Required(ErrorMessage = "Current password is required.")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "New password must be at least 6 characters long.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your new password.")]
    [Compare(nameof(NewPassword), ErrorMessage = "New passwords do not match.")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}

public sealed record AuthenticatedUserDto(
    int UserId,
    int? StudentId,
    string? StudentNumber,
    string FullName,
    string Email,
    string Role,
    bool IsActive);
