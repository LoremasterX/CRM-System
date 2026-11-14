using CRM_System.Core.Common;
using CRM_System.Core.Students.DTOs;
using CRM_System.Core.Students.Interfaces;
using CRM_System.Core.Students.Models;
using CRM_System.Core.Students.Validation;

namespace CRM_System.Core.Students.Services;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public StudentService(
        IStudentRepository studentRepository,
        IUserRepository userRepository,
        IPasswordHasher passwordHasher)
    {
        _studentRepository = studentRepository;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<ServiceResult<IReadOnlyList<StudentDto>>> GetStudentsAsync(
        StudentFilterDto? filter,
        AccessContext access,
        CancellationToken cancellationToken = default)
    {
        if (!access.IsAuthenticated)
        {
            return ServiceResult<IReadOnlyList<StudentDto>>.Failure(
                "Authentication is required to view student records.",
                ServiceErrorType.Unauthorized);
        }

        if (!access.IsAdmin)
        {
            return ServiceResult<IReadOnlyList<StudentDto>>.Failure(
                "Only administrators can view the full student directory.",
                ServiceErrorType.Forbidden);
        }

        var students = await _studentRepository.GetStudentsAsync(filter, cancellationToken);
        var dtos = students.Select(StudentDto.FromEntity).ToList();
        return ServiceResult<IReadOnlyList<StudentDto>>.Success(dtos);
    }

    public async Task<ServiceResult<StudentDto>> GetStudentByIdAsync(
        int id,
        AccessContext access,
        CancellationToken cancellationToken = default)
    {
        if (!access.IsAuthenticated)
        {
            return ServiceResult<StudentDto>.Failure("Authentication is required.", ServiceErrorType.Unauthorized);
        }

        var student = await _studentRepository.GetByIdAsync(id, cancellationToken);
        if (student is null)
        {
            return ServiceResult<StudentDto>.Failure($"Student with ID {id} was not found.", ServiceErrorType.NotFound);
        }

        if (!access.CanAccessStudent(student.Id, student.UserId))
        {
            return ServiceResult<StudentDto>.Failure(
                "You do not have permission to view another student's record.",
                ServiceErrorType.Forbidden);
        }

        return ServiceResult<StudentDto>.Success(StudentDto.FromEntity(student));
    }

    public async Task<ServiceResult<StudentDto>> GetStudentByUserIdAsync(
        int userId,
        AccessContext access,
        CancellationToken cancellationToken = default)
    {
        if (!access.IsAuthenticated)
        {
            return ServiceResult<StudentDto>.Failure("Authentication is required.", ServiceErrorType.Unauthorized);
        }

        if (!access.IsAdmin && access.UserId != userId)
        {
            return ServiceResult<StudentDto>.Failure(
                "You do not have permission to view another user's student profile.",
                ServiceErrorType.Forbidden);
        }

        var student = await _studentRepository.GetByUserIdAsync(userId, cancellationToken);
        if (student is null)
        {
            return ServiceResult<StudentDto>.Failure("No student profile is associated with this user account.", ServiceErrorType.NotFound);
        }

        return ServiceResult<StudentDto>.Success(StudentDto.FromEntity(student));
    }

    public async Task<ServiceResult<StudentDto>> CreateStudentAsync(
        CreateStudentRequestDto request,
        AccessContext access,
        CancellationToken cancellationToken = default)
    {
        if (!access.IsAuthenticated)
        {
            return ServiceResult<StudentDto>.Failure("Authentication is required.", ServiceErrorType.Unauthorized);
        }

        if (!access.IsAdmin)
        {
            return ServiceResult<StudentDto>.Failure(
                "Only administrators can create student records.",
                ServiceErrorType.Forbidden);
        }

        var validationErrors = StudentValidator.ValidateCreateStudent(request);
        if (validationErrors.Count > 0)
        {
            return ServiceResult<StudentDto>.Failure(validationErrors, ServiceErrorType.Validation);
        }

        var cleanEmail = request.Email.Trim().ToLowerInvariant();
        if (await _studentRepository.EmailExistsAsync(cleanEmail, null, cancellationToken))
        {
            return ServiceResult<StudentDto>.Failure(
                $"A student with email '{cleanEmail}' already exists.",
                ServiceErrorType.Conflict);
        }

        if (request.CreateLoginAccount && await _userRepository.EmailExistsAsync(cleanEmail, null, cancellationToken))
        {
            return ServiceResult<StudentDto>.Failure(
                $"A user account with email '{cleanEmail}' already exists.",
                ServiceErrorType.Conflict);
        }

        string studentNumber;
        if (!string.IsNullOrWhiteSpace(request.StudentNumber))
        {
            studentNumber = request.StudentNumber.Trim().ToUpperInvariant();
            if (await _studentRepository.StudentNumberExistsAsync(studentNumber, null, cancellationToken))
            {
                return ServiceResult<StudentDto>.Failure(
                    $"Student ID '{studentNumber}' already exists.",
                    ServiceErrorType.Conflict);
            }
        }
        else
        {
            studentNumber = await _studentRepository.GenerateNextStudentNumberAsync(cancellationToken);
        }

        var now = DateTime.UtcNow;
        int? createdUserId = null;

        if (request.CreateLoginAccount)
        {
            var password = string.IsNullOrWhiteSpace(request.InitialPassword)
                ? "Student123!"
                : request.InitialPassword;

            var user = new ApplicationUser
            {
                Email = cleanEmail,
                NormalizedEmail = cleanEmail.ToUpperInvariant(),
                FullName = request.FullName.Trim(),
                PasswordHash = _passwordHasher.HashPassword(password),
                Role = UserRoles.Student,
                IsActive = request.Status == StudentStatus.Active,
                CreatedAt = now
            };

            var createdUser = await _userRepository.AddAsync(user, cancellationToken);
            createdUserId = createdUser.Id;
        }

        var student = new Student
        {
            UserId = createdUserId,
            StudentNumber = studentNumber,
            FullName = request.FullName.Trim(),
            Email = cleanEmail,
            PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim(),
            DateOfBirth = request.DateOfBirth,
            University = request.University.Trim(),
            Faculty = request.Faculty.Trim(),
            Department = request.Department.Trim(),
            StudyProgram = request.StudyProgram.Trim(),
            StudyYear = request.StudyYear,
            Bio = string.IsNullOrWhiteSpace(request.Bio) ? null : request.Bio.Trim(),
            Status = request.Status,
            EnrollmentDate = request.EnrollmentDate ?? DateOnly.FromDateTime(now),
            CreatedAt = now,
            UpdatedAt = now
        };

        var createdStudent = await _studentRepository.AddAsync(student, cancellationToken);
        return ServiceResult<StudentDto>.Success(
            StudentDto.FromEntity(createdStudent),
            $"Student '{createdStudent.FullName}' ({createdStudent.StudentNumber}) created successfully.");
    }

    public async Task<ServiceResult<StudentDto>> UpdateStudentAsync(
        int id,
        UpdateStudentRequestDto request,
        AccessContext access,
        CancellationToken cancellationToken = default)
    {
        if (!access.IsAuthenticated)
        {
            return ServiceResult<StudentDto>.Failure("Authentication is required.", ServiceErrorType.Unauthorized);
        }

        if (!access.IsAdmin)
        {
            return ServiceResult<StudentDto>.Failure(
                "Only administrators can perform full administrative updates on student records.",
                ServiceErrorType.Forbidden);
        }

        var validationErrors = StudentValidator.ValidateUpdateStudent(request);
        if (validationErrors.Count > 0)
        {
            return ServiceResult<StudentDto>.Failure(validationErrors, ServiceErrorType.Validation);
        }

        var existing = await _studentRepository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return ServiceResult<StudentDto>.Failure($"Student with ID {id} was not found.", ServiceErrorType.NotFound);
        }

        var cleanEmail = request.Email.Trim().ToLowerInvariant();
        if (await _studentRepository.EmailExistsAsync(cleanEmail, id, cancellationToken))
        {
            return ServiceResult<StudentDto>.Failure(
                $"Another student record already uses email '{cleanEmail}'.",
                ServiceErrorType.Conflict);
        }

        var cleanNumber = request.StudentNumber.Trim().ToUpperInvariant();
        if (await _studentRepository.StudentNumberExistsAsync(cleanNumber, id, cancellationToken))
        {
            return ServiceResult<StudentDto>.Failure(
                $"Another student record already uses Student ID '{cleanNumber}'.",
                ServiceErrorType.Conflict);
        }

        if (existing.UserId.HasValue &&
            await _userRepository.EmailExistsAsync(cleanEmail, existing.UserId.Value, cancellationToken))
        {
            return ServiceResult<StudentDto>.Failure(
                $"Another user account already uses email '{cleanEmail}'.",
                ServiceErrorType.Conflict);
        }

        existing.FullName = request.FullName.Trim();
        existing.Email = cleanEmail;
        existing.StudentNumber = cleanNumber;
        existing.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
        existing.DateOfBirth = request.DateOfBirth;
        existing.University = request.University.Trim();
        existing.Faculty = request.Faculty.Trim();
        existing.Department = request.Department.Trim();
        existing.StudyProgram = request.StudyProgram.Trim();
        existing.StudyYear = request.StudyYear;
        existing.Bio = string.IsNullOrWhiteSpace(request.Bio) ? null : request.Bio.Trim();
        existing.Status = request.Status;
        existing.EnrollmentDate = request.EnrollmentDate;
        existing.UpdatedAt = DateTime.UtcNow;

        await _studentRepository.UpdateAsync(existing, cancellationToken);

        if (existing.UserId.HasValue)
        {
            var user = await _userRepository.GetByIdAsync(existing.UserId.Value, cancellationToken);
            if (user is not null)
            {
                user.FullName = existing.FullName;
                user.Email = cleanEmail;
                user.NormalizedEmail = cleanEmail.ToUpperInvariant();
                user.IsActive = existing.Status is StudentStatus.Active or StudentStatus.Graduated;
                await _userRepository.UpdateAsync(user, cancellationToken);
            }
        }

        return ServiceResult<StudentDto>.Success(
            StudentDto.FromEntity(existing),
            $"Student '{existing.FullName}' updated successfully.");
    }

    public async Task<ServiceResult<StudentDto>> UpdatePersonalProfileAsync(
        int studentId,
        UpdatePersonalProfileRequestDto request,
        AccessContext access,
        CancellationToken cancellationToken = default)
    {
        if (!access.IsAuthenticated)
        {
            return ServiceResult<StudentDto>.Failure("Authentication is required.", ServiceErrorType.Unauthorized);
        }

        var existing = await _studentRepository.GetByIdAsync(studentId, cancellationToken);
        if (existing is null)
        {
            return ServiceResult<StudentDto>.Failure($"Student with ID {studentId} was not found.", ServiceErrorType.NotFound);
        }

        if (!access.CanAccessStudent(existing.Id, existing.UserId))
        {
            return ServiceResult<StudentDto>.Failure(
                "You are only permitted to edit your own personal profile.",
                ServiceErrorType.Forbidden);
        }

        var validationErrors = StudentValidator.ValidatePersonalProfile(request);
        if (validationErrors.Count > 0)
        {
            return ServiceResult<StudentDto>.Failure(validationErrors, ServiceErrorType.Validation);
        }

        existing.FullName = request.FullName.Trim();
        existing.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
        existing.DateOfBirth = request.DateOfBirth;
        existing.University = request.University.Trim();
        existing.Faculty = request.Faculty.Trim();
        existing.Department = request.Department.Trim();
        existing.StudyProgram = request.StudyProgram.Trim();
        existing.StudyYear = request.StudyYear;
        existing.Bio = string.IsNullOrWhiteSpace(request.Bio) ? null : request.Bio.Trim();
        existing.UpdatedAt = DateTime.UtcNow;

        await _studentRepository.UpdateAsync(existing, cancellationToken);

        if (existing.UserId.HasValue)
        {
            var user = await _userRepository.GetByIdAsync(existing.UserId.Value, cancellationToken);
            if (user is not null)
            {
                user.FullName = existing.FullName;
                await _userRepository.UpdateAsync(user, cancellationToken);
            }
        }

        return ServiceResult<StudentDto>.Success(
            StudentDto.FromEntity(existing),
            "Personal profile updated successfully.");
    }

    public async Task<ServiceResult<StudentDto>> SetStudentStatusAsync(
        int id,
        StudentStatus status,
        AccessContext access,
        CancellationToken cancellationToken = default)
    {
        if (!access.IsAuthenticated)
        {
            return ServiceResult<StudentDto>.Failure("Authentication is required.", ServiceErrorType.Unauthorized);
        }

        if (!access.IsAdmin)
        {
            return ServiceResult<StudentDto>.Failure(
                "Only administrators can change student account status.",
                ServiceErrorType.Forbidden);
        }

        if (!Enum.IsDefined(typeof(StudentStatus), status))
        {
            return ServiceResult<StudentDto>.Failure("Invalid student status value.", ServiceErrorType.Validation);
        }

        var student = await _studentRepository.GetByIdAsync(id, cancellationToken);
        if (student is null)
        {
            return ServiceResult<StudentDto>.Failure($"Student with ID {id} was not found.", ServiceErrorType.NotFound);
        }

        student.Status = status;
        student.UpdatedAt = DateTime.UtcNow;
        await _studentRepository.UpdateAsync(student, cancellationToken);

        if (student.UserId.HasValue)
        {
            var user = await _userRepository.GetByIdAsync(student.UserId.Value, cancellationToken);
            if (user is not null)
            {
                user.IsActive = status is StudentStatus.Active or StudentStatus.Graduated;
                await _userRepository.UpdateAsync(user, cancellationToken);
            }
        }

        return ServiceResult<StudentDto>.Success(
            StudentDto.FromEntity(student),
            $"Student status changed to {status}.");
    }

    public async Task<ServiceResult> DeleteStudentAsync(
        int id,
        AccessContext access,
        CancellationToken cancellationToken = default)
    {
        if (!access.IsAuthenticated)
        {
            return ServiceResult.Failure("Authentication is required.", ServiceErrorType.Unauthorized);
        }

        if (!access.IsAdmin)
        {
            return ServiceResult.Failure(
                "Only administrators can delete student records.",
                ServiceErrorType.Forbidden);
        }

        var student = await _studentRepository.GetByIdAsync(id, cancellationToken);
        if (student is null)
        {
            return ServiceResult.Failure($"Student with ID {id} was not found.", ServiceErrorType.NotFound);
        }

        await _studentRepository.DeleteAsync(student, cancellationToken);
        return ServiceResult.Success($"Student '{student.FullName}' ({student.StudentNumber}) has been deleted.");
    }

    public async Task<ServiceResult<StudentDirectoryStatsDto>> GetDirectoryStatsAsync(
        AccessContext access,
        CancellationToken cancellationToken = default)
    {
        if (!access.IsAuthenticated)
        {
            return ServiceResult<StudentDirectoryStatsDto>.Failure("Authentication is required.", ServiceErrorType.Unauthorized);
        }

        var allStudents = await _studentRepository.GetStudentsAsync(null, cancellationToken);
        var stats = new StudentDirectoryStatsDto(
            TotalStudents: allStudents.Count,
            ActiveStudents: allStudents.Count(s => s.Status == StudentStatus.Active),
            InactiveStudents: allStudents.Count(s => s.Status == StudentStatus.Inactive),
            GraduatedStudents: allStudents.Count(s => s.Status == StudentStatus.Graduated),
            SuspendedStudents: allStudents.Count(s => s.Status == StudentStatus.Suspended),
            FacultiesCount: allStudents.Select(s => s.Faculty).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
            DepartmentsCount: allStudents.Select(s => s.Department).Distinct(StringComparer.OrdinalIgnoreCase).Count());

        return ServiceResult<StudentDirectoryStatsDto>.Success(stats);
    }

    public Task<IReadOnlyList<string>> GetDistinctFacultiesAsync(CancellationToken cancellationToken = default) =>
        _studentRepository.GetDistinctFacultiesAsync(cancellationToken);
}
