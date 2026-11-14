using System.Text.RegularExpressions;
using CRM_System.Core.Students.DTOs;
using CRM_System.Core.Students.Models;

namespace CRM_System.Core.Students.Validation;

public static partial class StudentValidator
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static IReadOnlyList<string> ValidateLogin(LoginRequestDto? request)
    {
        if (request is null)
        {
            return ["Login request cannot be null."];
        }

        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            errors.Add("Email address is required.");
        }
        else if (!EmailRegex.IsMatch(request.Email.Trim()))
        {
            errors.Add("Email address format is invalid.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            errors.Add("Password is required.");
        }

        return errors;
    }

    public static IReadOnlyList<string> ValidateRegistration(RegisterStudentRequestDto? request)
    {
        if (request is null)
        {
            return ["Registration request cannot be null."];
        }

        var errors = new List<string>();
        ValidateCommonIdentity(request.FullName, request.Email, errors);
        ValidatePasswordStrength(request.Password, request.ConfirmPassword, errors);
        ValidateAcademicFields(request.University, request.Faculty, request.Department, request.StudyProgram, request.StudyYear, errors);

        if (!string.IsNullOrWhiteSpace(request.StudentNumber) && request.StudentNumber.Trim().Length < 3)
        {
            errors.Add("Student ID must be at least 3 characters when provided.");
        }

        return errors;
    }

    public static IReadOnlyList<string> ValidateCreateStudent(CreateStudentRequestDto? request)
    {
        if (request is null)
        {
            return ["Create student request cannot be null."];
        }

        var errors = new List<string>();
        ValidateCommonIdentity(request.FullName, request.Email, errors);
        ValidateAcademicFields(request.University, request.Faculty, request.Department, request.StudyProgram, request.StudyYear, errors);
        ValidateOptionalMetadata(request.PhoneNumber, request.DateOfBirth, request.Bio, errors);

        if (!string.IsNullOrWhiteSpace(request.StudentNumber) && request.StudentNumber.Trim().Length < 3)
        {
            errors.Add("Student ID must be at least 3 characters when specified.");
        }

        if (request.CreateLoginAccount && !string.IsNullOrWhiteSpace(request.InitialPassword))
        {
            ValidatePasswordStrength(request.InitialPassword, request.InitialPassword, errors);
        }

        return errors;
    }

    public static IReadOnlyList<string> ValidateUpdateStudent(UpdateStudentRequestDto? request)
    {
        if (request is null)
        {
            return ["Update student request cannot be null."];
        }

        var errors = new List<string>();
        ValidateCommonIdentity(request.FullName, request.Email, errors);
        ValidateAcademicFields(request.University, request.Faculty, request.Department, request.StudyProgram, request.StudyYear, errors);
        ValidateOptionalMetadata(request.PhoneNumber, request.DateOfBirth, request.Bio, errors);

        if (string.IsNullOrWhiteSpace(request.StudentNumber) || request.StudentNumber.Trim().Length < 3)
        {
            errors.Add("Student ID is required and must be at least 3 characters.");
        }

        if (!Enum.IsDefined(typeof(StudentStatus), request.Status))
        {
            errors.Add("Invalid student status value.");
        }

        return errors;
    }

    public static IReadOnlyList<string> ValidatePersonalProfile(UpdatePersonalProfileRequestDto? request)
    {
        if (request is null)
        {
            return ["Profile update request cannot be null."];
        }

        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(request.FullName) || request.FullName.Trim().Length < 2)
        {
            errors.Add("Full name is required and must be at least 2 characters.");
        }
        else if (request.FullName.Trim().Length > 120)
        {
            errors.Add("Full name cannot exceed 120 characters.");
        }

        ValidateAcademicFields(request.University, request.Faculty, request.Department, request.StudyProgram, request.StudyYear, errors);
        ValidateOptionalMetadata(request.PhoneNumber, request.DateOfBirth, request.Bio, errors);

        return errors;
    }

    public static IReadOnlyList<string> ValidatePasswordChange(ChangePasswordRequestDto? request)
    {
        if (request is null)
        {
            return ["Password change request cannot be null."];
        }

        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(request.CurrentPassword))
        {
            errors.Add("Current password is required.");
        }

        ValidatePasswordStrength(request.NewPassword, request.ConfirmNewPassword, errors);
        return errors;
    }

    private static void ValidateCommonIdentity(string? fullName, string? email, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(fullName) || fullName.Trim().Length < 2)
        {
            errors.Add("Full name is required and must be at least 2 characters.");
        }
        else if (fullName.Trim().Length > 120)
        {
            errors.Add("Full name cannot exceed 120 characters.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            errors.Add("Email address is required.");
        }
        else if (!EmailRegex.IsMatch(email.Trim()))
        {
            errors.Add("Email address format is invalid.");
        }
        else if (email.Trim().Length > 160)
        {
            errors.Add("Email address cannot exceed 160 characters.");
        }
    }

    private static void ValidateAcademicFields(
        string? university,
        string? faculty,
        string? department,
        string? studyProgram,
        int studyYear,
        List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(university) || university.Trim().Length < 2)
        {
            errors.Add("University name is required.");
        }

        if (string.IsNullOrWhiteSpace(faculty) || faculty.Trim().Length < 2)
        {
            errors.Add("Faculty is required.");
        }

        if (string.IsNullOrWhiteSpace(department) || department.Trim().Length < 2)
        {
            errors.Add("Department is required.");
        }

        if (string.IsNullOrWhiteSpace(studyProgram) || studyProgram.Trim().Length < 2)
        {
            errors.Add("Study program is required.");
        }

        if (studyYear is < 1 or > 6)
        {
            errors.Add("Study year must be between 1 and 6.");
        }
    }

    private static void ValidateOptionalMetadata(string? phoneNumber, DateOnly? dateOfBirth, string? bio, List<string> errors)
    {
        if (!string.IsNullOrWhiteSpace(phoneNumber) && phoneNumber.Trim().Length > 32)
        {
            errors.Add("Phone number cannot exceed 32 characters.");
        }

        if (dateOfBirth.HasValue)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (dateOfBirth.Value > today.AddYears(-14) || dateOfBirth.Value < today.AddYears(-100))
            {
                errors.Add("Date of birth must correspond to a valid student age (between 14 and 100 years).");
            }
        }

        if (!string.IsNullOrWhiteSpace(bio) && bio.Length > 1000)
        {
            errors.Add("Profile bio cannot exceed 1000 characters.");
        }
    }

    private static void ValidatePasswordStrength(string? password, string? confirmPassword, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
        {
            errors.Add("Password must be at least 6 characters long.");
            return;
        }

        if (!password.Any(char.IsLetter) || !password.Any(char.IsDigit))
        {
            errors.Add("Password must contain at least one letter and one digit.");
        }

        if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
        {
            errors.Add("Password and confirmation password do not match.");
        }
    }
}
