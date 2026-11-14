using CRM_System.Core.Students.DTOs;
using CRM_System.Core.Students.Validation;

namespace CRM_System.Tests.Students;

public class StudentValidatorTests
{
    [Fact]
    public void ValidateRegistration_InvalidFields_ReturnsDescriptiveErrors()
    {
        var invalid = new RegisterStudentRequestDto
        {
            FullName = "A",
            Email = "not-an-email",
            Password = "short",
            ConfirmPassword = "mismatch",
            University = "",
            Faculty = "",
            Department = "",
            StudyProgram = "",
            StudyYear = 9
        };

        var errors = StudentValidator.ValidateRegistration(invalid);

        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Contains("Full name", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(errors, e => e.Contains("Email", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(errors, e => e.Contains("Password", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(errors, e => e.Contains("Study year", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ValidateCreateStudent_InvalidDateOfBirth_ReturnsAgeValidationError()
    {
        var dto = new CreateStudentRequestDto
        {
            FullName = "Valid Name",
            Email = "valid@student.crm.edu",
            DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        var errors = StudentValidator.ValidateCreateStudent(dto);

        Assert.Single(errors);
        Assert.Contains("Date of birth", errors[0], StringComparison.OrdinalIgnoreCase);
    }
}
