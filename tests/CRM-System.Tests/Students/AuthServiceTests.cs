using CRM_System.Core.Common;
using CRM_System.Core.Students.DTOs;
using CRM_System.Core.Students.Models;
using CRM_System.Core.Students.Services;
using CRM_System.Infrastructure.Data;
using CRM_System.Infrastructure.Students;
using Microsoft.EntityFrameworkCore;

namespace CRM_System.Tests.Students;

public class AuthServiceTests
{
    private static (CrmDbContext Db, AuthService Auth, PasswordHasherAdapter Hasher) CreateSut()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"CrmAuthTests_{Guid.NewGuid():N}")
            .Options;

        var db = new CrmDbContext(options);
        var hasher = new PasswordHasherAdapter();
        var userRepo = new UserRepository(db);
        var studentRepo = new StudentRepository(db);
        var authService = new AuthService(userRepo, studentRepo, hasher);

        return (db, authService, hasher);
    }

    [Fact]
    public async Task RegisterStudentAsync_ValidInput_HashesPasswordAndCreatesUserAndStudentProfile()
    {
        var (db, auth, hasher) = CreateSut();
        var request = new RegisterStudentRequestDto
        {
            FullName = "Nursultan Karimov",
            Email = "nursultan.karimov@student.crm.edu",
            Password = "SecurePassword123",
            ConfirmPassword = "SecurePassword123",
            University = "Astana IT University",
            Faculty = "School of Software Engineering",
            Department = "Computer Science",
            StudyProgram = "B.Sc. Software Engineering",
            StudyYear = 2,
            PhoneNumber = "+7 701 111 2233",
            Bio = "Interested in backend architecture."
        };

        var result = await auth.RegisterStudentAsync(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(UserRoles.Student, result.Data!.Role);
        Assert.NotNull(result.Data.StudentId);
        Assert.StartsWith("STU-", result.Data.StudentNumber);

        var storedUser = await db.Users.SingleAsync();
        Assert.NotEqual("SecurePassword123", storedUser.PasswordHash);
        Assert.True(hasher.VerifyPassword("SecurePassword123", storedUser.PasswordHash));

        var storedStudent = await db.Students.SingleAsync();
        Assert.Equal("Nursultan Karimov", storedStudent.FullName);
        Assert.Equal(storedUser.Id, storedStudent.UserId);
    }

    [Fact]
    public async Task RegisterStudentAsync_DuplicateEmail_ReturnsConflictFailure()
    {
        var (_, auth, _) = CreateSut();
        var request = new RegisterStudentRequestDto
        {
            FullName = "First Student",
            Email = "duplicate@student.crm.edu",
            Password = "Password123",
            ConfirmPassword = "Password123"
        };

        var first = await auth.RegisterStudentAsync(request);
        var second = await auth.RegisterStudentAsync(request);

        Assert.True(first.Succeeded);
        Assert.False(second.Succeeded);
        Assert.Equal(ServiceErrorType.Conflict, second.ErrorType);
    }

    [Fact]
    public async Task LoginAsync_ValidAndInvalidCredentials_EnforcesAuthenticationRules()
    {
        var (_, auth, _) = CreateSut();
        await auth.RegisterStudentAsync(new RegisterStudentRequestDto
        {
            FullName = "Aigerim Toleuova",
            Email = "aigerim@student.crm.edu",
            Password = "Password123",
            ConfirmPassword = "Password123"
        });

        var validLogin = await auth.LoginAsync(new LoginRequestDto
        {
            Email = "aigerim@student.crm.edu",
            Password = "Password123"
        });

        var invalidPasswordLogin = await auth.LoginAsync(new LoginRequestDto
        {
            Email = "aigerim@student.crm.edu",
            Password = "WrongPassword999"
        });

        var unknownEmailLogin = await auth.LoginAsync(new LoginRequestDto
        {
            Email = "nonexistent@student.crm.edu",
            Password = "Password123"
        });

        Assert.True(validLogin.Succeeded);
        Assert.Equal("Aigerim Toleuova", validLogin.Data!.FullName);

        Assert.False(invalidPasswordLogin.Succeeded);
        Assert.Equal(ServiceErrorType.Unauthorized, invalidPasswordLogin.ErrorType);

        Assert.False(unknownEmailLogin.Succeeded);
        Assert.Equal(ServiceErrorType.Unauthorized, unknownEmailLogin.ErrorType);
    }

    [Fact]
    public async Task LoginAsync_DeactivatedStudent_ReturnsForbidden()
    {
        var (db, auth, _) = CreateSut();
        var reg = await auth.RegisterStudentAsync(new RegisterStudentRequestDto
        {
            FullName = "Suspended Student",
            Email = "suspended@student.crm.edu",
            Password = "Password123",
            ConfirmPassword = "Password123"
        });

        var student = await db.Students.SingleAsync(s => s.Id == reg.Data!.StudentId);
        student.Status = StudentStatus.Suspended;
        await db.SaveChangesAsync();

        var loginResult = await auth.LoginAsync(new LoginRequestDto
        {
            Email = "suspended@student.crm.edu",
            Password = "Password123"
        });

        Assert.False(loginResult.Succeeded);
        Assert.Equal(ServiceErrorType.Forbidden, loginResult.ErrorType);
    }

    [Fact]
    public async Task ChangePasswordAsync_ValidAndInvalidCurrentPassword_BehavesCorrectly()
    {
        var (_, auth, _) = CreateSut();
        var reg = await auth.RegisterStudentAsync(new RegisterStudentRequestDto
        {
            FullName = "Password Changer",
            Email = "changer@student.crm.edu",
            Password = "OldPassword123",
            ConfirmPassword = "OldPassword123"
        });

        var failedAttempt = await auth.ChangePasswordAsync(reg.Data!.UserId, new ChangePasswordRequestDto
        {
            CurrentPassword = "WrongCurrentPassword1",
            NewPassword = "NewPassword456",
            ConfirmNewPassword = "NewPassword456"
        });

        var successAttempt = await auth.ChangePasswordAsync(reg.Data.UserId, new ChangePasswordRequestDto
        {
            CurrentPassword = "OldPassword123",
            NewPassword = "NewPassword456",
            ConfirmNewPassword = "NewPassword456"
        });

        var loginWithNewPassword = await auth.LoginAsync(new LoginRequestDto
        {
            Email = "changer@student.crm.edu",
            Password = "NewPassword456"
        });

        Assert.False(failedAttempt.Succeeded);
        Assert.Equal(ServiceErrorType.Unauthorized, failedAttempt.ErrorType);
        Assert.True(successAttempt.Succeeded);
        Assert.True(loginWithNewPassword.Succeeded);
    }
}
