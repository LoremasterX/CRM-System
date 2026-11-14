using CRM_System.Core.Common;
using CRM_System.Core.Students.DTOs;
using CRM_System.Core.Students.Interfaces;
using CRM_System.Core.Students.Models;
using CRM_System.Core.Students.Validation;

namespace CRM_System.Core.Students.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(
        IUserRepository userRepository,
        IStudentRepository studentRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _studentRepository = studentRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<ServiceResult<AuthenticatedUserDto>> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var validationErrors = StudentValidator.ValidateLogin(request);
        if (validationErrors.Count > 0)
        {
            return ServiceResult<AuthenticatedUserDto>.Failure(validationErrors, ServiceErrorType.Validation);
        }

        var normalizedEmail = request.Email.Trim().ToUpperInvariant();
        var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (user is null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return ServiceResult<AuthenticatedUserDto>.Failure(
                "Invalid email address or password.",
                ServiceErrorType.Unauthorized);
        }

        if (!user.IsActive)
        {
            return ServiceResult<AuthenticatedUserDto>.Failure(
                "Your account has been deactivated. Please contact an administrator.",
                ServiceErrorType.Forbidden);
        }

        var student = user.StudentProfile ?? await _studentRepository.GetByUserIdAsync(user.Id, cancellationToken);
        if (student is not null && student.Status is StudentStatus.Inactive or StudentStatus.Suspended)
        {
            return ServiceResult<AuthenticatedUserDto>.Failure(
                $"Student account status is {student.Status}. Access is currently restricted.",
                ServiceErrorType.Forbidden);
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, cancellationToken);

        var dto = new AuthenticatedUserDto(
            user.Id,
            student?.Id,
            student?.StudentNumber,
            user.FullName,
            user.Email,
            user.Role,
            user.IsActive);

        return ServiceResult<AuthenticatedUserDto>.Success(dto, "Login successful.");
    }

    public async Task<ServiceResult<AuthenticatedUserDto>> RegisterStudentAsync(
        RegisterStudentRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var validationErrors = StudentValidator.ValidateRegistration(request);
        if (validationErrors.Count > 0)
        {
            return ServiceResult<AuthenticatedUserDto>.Failure(validationErrors, ServiceErrorType.Validation);
        }

        var cleanEmail = request.Email.Trim().ToLowerInvariant();
        if (await _userRepository.EmailExistsAsync(cleanEmail, null, cancellationToken) ||
            await _studentRepository.EmailExistsAsync(cleanEmail, null, cancellationToken))
        {
            return ServiceResult<AuthenticatedUserDto>.Failure(
                "An account or student record with this email address already exists.",
                ServiceErrorType.Conflict);
        }

        string studentNumber;
        if (!string.IsNullOrWhiteSpace(request.StudentNumber))
        {
            studentNumber = request.StudentNumber.Trim().ToUpperInvariant();
            if (await _studentRepository.StudentNumberExistsAsync(studentNumber, null, cancellationToken))
            {
                return ServiceResult<AuthenticatedUserDto>.Failure(
                    $"Student ID '{studentNumber}' is already assigned to another student.",
                    ServiceErrorType.Conflict);
            }
        }
        else
        {
            studentNumber = await _studentRepository.GenerateNextStudentNumberAsync(cancellationToken);
        }

        var now = DateTime.UtcNow;
        var user = new ApplicationUser
        {
            Email = cleanEmail,
            NormalizedEmail = cleanEmail.ToUpperInvariant(),
            FullName = request.FullName.Trim(),
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            Role = UserRoles.Student,
            IsActive = true,
            CreatedAt = now,
            LastLoginAt = now
        };

        var createdUser = await _userRepository.AddAsync(user, cancellationToken);

        var student = new Student
        {
            UserId = createdUser.Id,
            StudentNumber = studentNumber,
            FullName = request.FullName.Trim(),
            Email = cleanEmail,
            PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim(),
            University = request.University.Trim(),
            Faculty = request.Faculty.Trim(),
            Department = request.Department.Trim(),
            StudyProgram = request.StudyProgram.Trim(),
            StudyYear = request.StudyYear,
            Bio = string.IsNullOrWhiteSpace(request.Bio) ? null : request.Bio.Trim(),
            Status = StudentStatus.Active,
            EnrollmentDate = DateOnly.FromDateTime(now),
            CreatedAt = now,
            UpdatedAt = now
        };

        var createdStudent = await _studentRepository.AddAsync(student, cancellationToken);

        var dto = new AuthenticatedUserDto(
            createdUser.Id,
            createdStudent.Id,
            createdStudent.StudentNumber,
            createdUser.FullName,
            createdUser.Email,
            createdUser.Role,
            createdUser.IsActive);

        return ServiceResult<AuthenticatedUserDto>.Success(dto, "Student registration completed successfully.");
    }

    public async Task<ServiceResult> ChangePasswordAsync(
        int userId,
        ChangePasswordRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var validationErrors = StudentValidator.ValidatePasswordChange(request);
        if (validationErrors.Count > 0)
        {
            return ServiceResult.Failure(validationErrors, ServiceErrorType.Validation);
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return ServiceResult.Failure("User account was not found.", ServiceErrorType.NotFound);
        }

        if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            return ServiceResult.Failure("Current password is incorrect.", ServiceErrorType.Unauthorized);
        }

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        await _userRepository.UpdateAsync(user, cancellationToken);

        return ServiceResult.Success("Password updated successfully.");
    }

    public async Task<AuthenticatedUserDto?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var student = user.StudentProfile ?? await _studentRepository.GetByUserIdAsync(user.Id, cancellationToken);
        return new AuthenticatedUserDto(
            user.Id,
            student?.Id,
            student?.StudentNumber,
            user.FullName,
            user.Email,
            user.Role,
            user.IsActive);
    }
}
