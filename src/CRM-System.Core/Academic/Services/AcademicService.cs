using CRM_System.Core.Academic.DTOs;
using CRM_System.Core.Academic.Interfaces;
using CRM_System.Core.Academic.Models;
using CRM_System.Core.Academic.Validation;
using CRM_System.Core.Common;
using CRM_System.Core.Students.Interfaces;

namespace CRM_System.Core.Academic.Services;

public class AcademicService : IAcademicService
{
    private readonly IAcademicRepository _academicRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IGpaCalculator _gpaCalculator;

    public AcademicService(
        IAcademicRepository academicRepository,
        IStudentRepository studentRepository,
        IGpaCalculator gpaCalculator)
    {
        _academicRepository = academicRepository;
        _studentRepository = studentRepository;
        _gpaCalculator = gpaCalculator;
    }

    // =========================================================================
    // TEACHERS
    // =========================================================================

    public async Task<ServiceResult<IReadOnlyList<TeacherDto>>> GetTeachersAsync(
        AccessContext access,
        CancellationToken cancellationToken = default)
    {
        if (!access.IsAuthenticated)
        {
            return ServiceResult<IReadOnlyList<TeacherDto>>.Failure("Authentication is required.", ServiceErrorType.Unauthorized);
        }

        var teachers = await _academicRepository.GetTeachersAsync(cancellationToken);
        var dtos = teachers.Select(MapTeacherDto).ToList();
        return ServiceResult<IReadOnlyList<TeacherDto>>.Success(dtos);
    }

    public async Task<ServiceResult<TeacherDto>> CreateTeacherAsync(
        SaveTeacherRequestDto request,
        AccessContext access,
        CancellationToken cancellationToken = default)
    {
        if (!access.IsAuthenticated)
        {
            return ServiceResult<TeacherDto>.Failure("Authentication is required.", ServiceErrorType.Unauthorized);
        }

        if (!access.IsAdmin)
        {
            return ServiceResult<TeacherDto>.Failure("Only administrators can create teacher profiles.", ServiceErrorType.Forbidden);
        }

        var errors = AcademicValidator.ValidateTeacher(request);
        if (errors.Count > 0)
        {
            return ServiceResult<TeacherDto>.Failure(errors, ServiceErrorType.Validation);
        }

        var cleanEmail = request.Email.Trim().ToLowerInvariant();
        if (await _academicRepository.TeacherEmailExistsAsync(cleanEmail, null, cancellationToken))
        {
            return ServiceResult<TeacherDto>.Failure($"A teacher with email '{cleanEmail}' already exists.", ServiceErrorType.Conflict);
        }

        var teacher = new Teacher
        {
            FullName = request.FullName.Trim(),
            Email = cleanEmail,
            Department = request.Department.Trim(),
            AcademicTitle = request.AcademicTitle.Trim(),
            OfficeRoom = string.IsNullOrWhiteSpace(request.OfficeRoom) ? null : request.OfficeRoom.Trim(),
            PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim(),
            Bio = string.IsNullOrWhiteSpace(request.Bio) ? null : request.Bio.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        var created = await _academicRepository.AddTeacherAsync(teacher, cancellationToken);
        return ServiceResult<TeacherDto>.Success(MapTeacherDto(created), $"Teacher '{created.FullName}' created successfully.");
    }

    public async Task<ServiceResult<TeacherDto>> UpdateTeacherAsync(
        int id,
        SaveTeacherRequestDto request,
        AccessContext access,
        CancellationToken cancellationToken = default)
    {
        if (!access.IsAuthenticated)
        {
            return ServiceResult<TeacherDto>.Failure("Authentication is required.", ServiceErrorType.Unauthorized);
        }

        if (!access.IsAdmin)
        {
            return ServiceResult<TeacherDto>.Failure("Only administrators can update teacher profiles.", ServiceErrorType.Forbidden);
        }

        var errors = AcademicValidator.ValidateTeacher(request);
        if (errors.Count > 0)
        {
            return ServiceResult<TeacherDto>.Failure(errors, ServiceErrorType.Validation);
        }

        var existing = await _academicRepository.GetTeacherByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return ServiceResult<TeacherDto>.Failure($"Teacher with ID {id} was not found.", ServiceErrorType.NotFound);
        }

        var cleanEmail = request.Email.Trim().ToLowerInvariant();
        if (await _academicRepository.TeacherEmailExistsAsync(cleanEmail, id, cancellationToken))
        {
            return ServiceResult<TeacherDto>.Failure($"Another teacher already uses email '{cleanEmail}'.", ServiceErrorType.Conflict);
        }

        existing.FullName = request.FullName.Trim();
        existing.Email = cleanEmail;
        existing.Department = request.Department.Trim();
        existing.AcademicTitle = request.AcademicTitle.Trim();
        existing.OfficeRoom = string.IsNullOrWhiteSpace(request.OfficeRoom) ? null : request.OfficeRoom.Trim();
        existing.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
        existing.Bio = string.IsNullOrWhiteSpace(request.Bio) ? null : request.Bio.Trim();

        await _academicRepository.UpdateTeacherAsync(existing, cancellationToken);
        return ServiceResult<TeacherDto>.Success(MapTeacherDto(existing), "Teacher profile updated.");
    }

    public async Task<ServiceResult> DeleteTeacherAsync(
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
            return ServiceResult.Failure("Only administrators can delete teacher profiles.", ServiceErrorType.Forbidden);
        }

        var existing = await _academicRepository.GetTeacherByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return ServiceResult.Failure($"Teacher with ID {id} was not found.", ServiceErrorType.NotFound);
        }

        await _academicRepository.DeleteTeacherAsync(existing, cancellationToken);
        return ServiceResult.Success($"Teacher '{existing.FullName}' deleted.");
    }

    // =========================================================================
    // COURSES
    // =========================================================================

    public async Task<ServiceResult<IReadOnlyList<CourseDto>>> GetCoursesAsync(
        string? searchTerm,
        string? department,
        AccessContext access,
        CancellationToken cancellationToken = default)
    {
        if (!access.IsAuthenticated)
        {
            return ServiceResult<IReadOnlyList<CourseDto>>.Failure("Authentication is required.", ServiceErrorType.Unauthorized);
        }

        var courses = await _academicRepository.GetCoursesAsync(searchTerm, department, cancellationToken);
        var dtos = courses.Select(MapCourseDto).ToList();
        return ServiceResult<IReadOnlyList<CourseDto>>.Success(dtos);
    }

    public async Task<ServiceResult<CourseDto>> GetCourseByIdAsync(
        int id,
        AccessContext access,
        CancellationToken cancellationToken = default)
    {
        if (!access.IsAuthenticated)
        {
            return ServiceResult<CourseDto>.Failure("Authentication is required.", ServiceErrorType.Unauthorized);
        }

        var course = await _academicRepository.GetCourseByIdAsync(id, cancellationToken);
        if (course is null)
        {
            return ServiceResult<CourseDto>.Failure($"Course with ID {id} was not found.", ServiceErrorType.NotFound);
        }

        return ServiceResult<CourseDto>.Success(MapCourseDto(course));
    }

    public async Task<ServiceResult<CourseDto>> CreateCourseAsync(
        SaveCourseRequestDto request,
        AccessContext access,
        CancellationToken cancellationToken = default)
    {
        if (!access.IsAuthenticated)
        {
            return ServiceResult<CourseDto>.Failure("Authentication is required.", ServiceErrorType.Unauthorized);
        }

        if (!access.IsAdmin)
        {
            return ServiceResult<CourseDto>.Failure("Only administrators can create courses.", ServiceErrorType.Forbidden);
        }

        var errors = AcademicValidator.ValidateCourse(request);
        if (errors.Count > 0)
        {
            return ServiceResult<CourseDto>.Failure(errors, ServiceErrorType.Validation);
        }

        var cleanCode = request.CourseCode.Trim().ToUpperInvariant();
        if (await _academicRepository.CourseCodeExistsAsync(cleanCode, null, cancellationToken))
        {
            return ServiceResult<CourseDto>.Failure($"A course with code '{cleanCode}' already exists.", ServiceErrorType.Conflict);
        }

        if (request.TeacherId.HasValue)
        {
            var teacher = await _academicRepository.GetTeacherByIdAsync(request.TeacherId.Value, cancellationToken);
            if (teacher is null)
            {
                return ServiceResult<CourseDto>.Failure("Selected teacher does not exist.", ServiceErrorType.NotFound);
            }
        }

        var course = new Course
        {
            CourseCode = cleanCode,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Credits = request.Credits,
            Department = request.Department.Trim(),
            Semester = request.Semester.Trim(),
            TeacherId = request.TeacherId,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _academicRepository.AddCourseAsync(course, cancellationToken);
        var reloaded = await _academicRepository.GetCourseByIdAsync(created.Id, cancellationToken) ?? created;
        return ServiceResult<CourseDto>.Success(MapCourseDto(reloaded), $"Course '{reloaded.CourseCode} - {reloaded.Title}' created.");
    }

    public async Task<ServiceResult<CourseDto>> UpdateCourseAsync(
        int id,
        SaveCourseRequestDto request,
        AccessContext access,
        CancellationToken cancellationToken = default)
    {
        if (!access.IsAuthenticated)
        {
            return ServiceResult<CourseDto>.Failure("Authentication is required.", ServiceErrorType.Unauthorized);
        }

        if (!access.IsAdmin)
        {
            return ServiceResult<CourseDto>.Failure("Only administrators can edit courses.", ServiceErrorType.Forbidden);
        }

        var errors = AcademicValidator.ValidateCourse(request);
        if (errors.Count > 0)
        {
            return ServiceResult<CourseDto>.Failure(errors, ServiceErrorType.Validation);
        }

        var existing = await _academicRepository.GetCourseByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return ServiceResult<CourseDto>.Failure($"Course with ID {id} was not found.", ServiceErrorType.NotFound);
        }

        var cleanCode = request.CourseCode.Trim().ToUpperInvariant();
        if (await _academicRepository.CourseCodeExistsAsync(cleanCode, id, cancellationToken))
        {
            return ServiceResult<CourseDto>.Failure($"Another course already uses code '{cleanCode}'.", ServiceErrorType.Conflict);
        }

        existing.CourseCode = cleanCode;
        existing.Title = request.Title.Trim();
        existing.Description = request.Description.Trim();
        existing.Credits = request.Credits;
        existing.Department = request.Department.Trim();
        existing.Semester = request.Semester.Trim();
        existing.TeacherId = request.TeacherId;
        existing.IsActive = request.IsActive;

        await _academicRepository.UpdateCourseAsync(existing, cancellationToken);
        var reloaded = await _academicRepository.GetCourseByIdAsync(existing.Id, cancellationToken) ?? existing;
        return ServiceResult<CourseDto>.Success(MapCourseDto(reloaded), $"Course '{reloaded.CourseCode}' updated.");
    }

    public async Task<ServiceResult> DeleteCourseAsync(
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
            return ServiceResult.Failure("Only administrators can delete courses.", ServiceErrorType.Forbidden);
        }

        var existing = await _academicRepository.GetCourseByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return ServiceResult.Failure($"Course with ID {id} was not found.", ServiceErrorType.NotFound);
        }

        await _academicRepository.DeleteCourseAsync(existing, cancellationToken);
        return ServiceResult.Success($"Course '{existing.CourseCode}' deleted.");
    }

    // =========================================================================
    // ENROLLMENTS
    // =========================================================================

    public async Task<ServiceResult<IReadOnlyList<EnrollmentDto>>> GetEnrollmentsAsync(
        int? studentId,
        int? courseId,
        AccessContext access,
        CancellationToken cancellationToken = default)
    {
        if (!access.IsAuthenticated)
        {
            return ServiceResult<IReadOnlyList<EnrollmentDto>>.Failure("Authentication is required.", ServiceErrorType.Unauthorized);
        }

        if (!access.IsAdmin)
        {
            var effectiveStudentId = studentId ?? access.StudentId;
            if (!effectiveStudentId.HasValue || !access.CanAccessStudent(effectiveStudentId.Value))
            {
                return ServiceResult<IReadOnlyList<EnrollmentDto>>.Failure(
                    "Students may only view their own course enrollments.",
                    ServiceErrorType.Forbidden);
            }
            studentId = effectiveStudentId.Value;
        }

        var enrollments = await _academicRepository.GetEnrollmentsAsync(studentId, courseId, cancellationToken);
        return ServiceResult<IReadOnlyList<EnrollmentDto>>.Success(enrollments.Select(MapEnrollmentDto).ToList());
    }

    public async Task<ServiceResult<EnrollmentDto>> EnrollStudentAsync(
        CreateEnrollmentRequestDto request,
        AccessContext access,
        CancellationToken cancellationToken = default)
    {
        if (!access.IsAuthenticated)
        {
            return ServiceResult<EnrollmentDto>.Failure("Authentication is required.", ServiceErrorType.Unauthorized);
        }

        if (!access.IsAdmin && !access.CanAccessStudent(request.StudentId))
        {
            return ServiceResult<EnrollmentDto>.Failure(
                "You do not have permission to enroll another student.",
                ServiceErrorType.Forbidden);
        }

        var student = await _studentRepository.GetByIdAsync(request.StudentId, cancellationToken);
        if (student is null)
        {
            return ServiceResult<EnrollmentDto>.Failure("Student was not found.", ServiceErrorType.NotFound);
        }

        var course = await _academicRepository.GetCourseByIdAsync(request.CourseId, cancellationToken);
        if (course is null)
        {
            return ServiceResult<EnrollmentDto>.Failure("Course was not found.", ServiceErrorType.NotFound);
        }

        if (!course.IsActive)
        {
            return ServiceResult<EnrollmentDto>.Failure("Cannot enroll in an inactive course.", ServiceErrorType.Validation);
        }

        var existing = await _academicRepository.GetEnrollmentAsync(request.StudentId, request.CourseId, cancellationToken);
        if (existing is not null)
        {
            if (existing.Status == EnrollmentStatus.Dropped)
            {
                existing.Status = EnrollmentStatus.Enrolled;
                existing.EnrolledAt = DateTime.UtcNow;
                await _academicRepository.UpdateEnrollmentAsync(existing, cancellationToken);
                var updated = await _academicRepository.GetEnrollmentByIdAsync(existing.Id, cancellationToken) ?? existing;
                return ServiceResult<EnrollmentDto>.Success(MapEnrollmentDto(updated), $"Re-enrolled in '{course.CourseCode}'.");
            }

            return ServiceResult<EnrollmentDto>.Failure(
                $"Student '{student.FullName}' is already enrolled in '{course.CourseCode}'.",
                ServiceErrorType.Conflict);
        }

        var enrollment = new Enrollment
        {
            StudentId = request.StudentId,
            CourseId = request.CourseId,
            Semester = string.IsNullOrWhiteSpace(request.Semester) ? course.Semester : request.Semester.Trim(),
            Status = EnrollmentStatus.Enrolled,
            EnrolledAt = DateTime.UtcNow
        };

        var created = await _academicRepository.AddEnrollmentAsync(enrollment, cancellationToken);
        var loaded = await _academicRepository.GetEnrollmentByIdAsync(created.Id, cancellationToken) ?? created;
        return ServiceResult<EnrollmentDto>.Success(
            MapEnrollmentDto(loaded),
            $"Successfully enrolled '{student.FullName}' in '{course.CourseCode} - {course.Title}'.");
    }

    public async Task<ServiceResult<EnrollmentDto>> UpdateEnrollmentStatusAsync(
        int enrollmentId,
        EnrollmentStatus status,
        AccessContext access,
        CancellationToken cancellationToken = default)
    {
        if (!access.IsAuthenticated)
        {
            return ServiceResult<EnrollmentDto>.Failure("Authentication is required.", ServiceErrorType.Unauthorized);
        }

        var existing = await _academicRepository.GetEnrollmentByIdAsync(enrollmentId, cancellationToken);
        if (existing is null)
        {
            return ServiceResult<EnrollmentDto>.Failure("Enrollment record was not found.", ServiceErrorType.NotFound);
        }

        if (!access.IsAdmin && !access.CanAccessStudent(existing.StudentId))
        {
            return ServiceResult<EnrollmentDto>.Failure("You are not authorized to modify this enrollment.", ServiceErrorType.Forbidden);
        }

        existing.Status = status;
        await _academicRepository.UpdateEnrollmentAsync(existing, cancellationToken);
        return ServiceResult<EnrollmentDto>.Success(MapEnrollmentDto(existing), $"Enrollment status updated to {status}.");
    }

    public async Task<ServiceResult> RemoveEnrollmentAsync(
        int enrollmentId,
        AccessContext access,
        CancellationToken cancellationToken = default)
    {
        if (!access.IsAuthenticated)
        {
            return ServiceResult.Failure("Authentication is required.", ServiceErrorType.Unauthorized);
        }

        var existing = await _academicRepository.GetEnrollmentByIdAsync(enrollmentId, cancellationToken);
        if (existing is null)
        {
            return ServiceResult.Failure("Enrollment record was not found.", ServiceErrorType.NotFound);
        }

        if (!access.IsAdmin && !access.CanAccessStudent(existing.StudentId))
        {
            return ServiceResult.Failure("You are not authorized to remove this enrollment.", ServiceErrorType.Forbidden);
        }

        await _academicRepository.DeleteEnrollmentAsync(existing, cancellationToken);
        return ServiceResult.Success("Enrollment removed.");
    }

    // =========================================================================
    // GRADES & GPA
    // =========================================================================

    public async Task<ServiceResult<IReadOnlyList<GradeDto>>> GetGradesAsync(
        int? studentId,
        int? courseId,
        AccessContext access,
        CancellationToken cancellationToken = default)
    {
        if (!access.IsAuthenticated)
        {
            return ServiceResult<IReadOnlyList<GradeDto>>.Failure("Authentication is required.", ServiceErrorType.Unauthorized);
        }

        if (!access.IsAdmin)
        {
            var effectiveStudentId = studentId ?? access.StudentId;
            if (!effectiveStudentId.HasValue || !access.CanAccessStudent(effectiveStudentId.Value))
            {
                return ServiceResult<IReadOnlyList<GradeDto>>.Failure(
                    "Students may only view their own grades.",
                    ServiceErrorType.Forbidden);
            }
            studentId = effectiveStudentId.Value;
        }

        var grades = await _academicRepository.GetGradesAsync(studentId, courseId, cancellationToken);
        var dtos = grades.Select(MapGradeDto).ToList();
        return ServiceResult<IReadOnlyList<GradeDto>>.Success(dtos);
    }

    public async Task<ServiceResult<GradeDto>> AddGradeAsync(
        SaveGradeRequestDto request,
        AccessContext access,
        CancellationToken cancellationToken = default)
    {
        if (!access.IsAuthenticated)
        {
            return ServiceResult<GradeDto>.Failure("Authentication is required.", ServiceErrorType.Unauthorized);
        }

        if (!access.IsAdmin)
        {
            return ServiceResult<GradeDto>.Failure("Only administrators can record grades.", ServiceErrorType.Forbidden);
        }

        var errors = AcademicValidator.ValidateGrade(request);
        if (errors.Count > 0)
        {
            return ServiceResult<GradeDto>.Failure(errors, ServiceErrorType.Validation);
        }

        var student = await _studentRepository.GetByIdAsync(request.StudentId, cancellationToken);
        if (student is null)
        {
            return ServiceResult<GradeDto>.Failure("Selected student does not exist.", ServiceErrorType.NotFound);
        }

        var course = await _academicRepository.GetCourseByIdAsync(request.CourseId, cancellationToken);
        if (course is null)
        {
            return ServiceResult<GradeDto>.Failure("Selected course does not exist.", ServiceErrorType.NotFound);
        }

        var grade = new Grade
        {
            StudentId = request.StudentId,
            CourseId = request.CourseId,
            AssessmentTitle = request.AssessmentTitle.Trim(),
            AssessmentType = string.IsNullOrWhiteSpace(request.AssessmentType) ? "Exam" : request.AssessmentType.Trim(),
            Score = Math.Round(request.Score, 2),
            Weight = request.Weight,
            Feedback = string.IsNullOrWhiteSpace(request.Feedback) ? null : request.Feedback.Trim(),
            GradedAt = DateTime.UtcNow
        };

        var created = await _academicRepository.AddGradeAsync(grade, cancellationToken);
        var loaded = await _academicRepository.GetGradeByIdAsync(created.Id, cancellationToken) ?? created;
        return ServiceResult<GradeDto>.Success(MapGradeDto(loaded), "Grade recorded successfully.");
    }

    public async Task<ServiceResult<GradeDto>> UpdateGradeAsync(
        int gradeId,
        SaveGradeRequestDto request,
        AccessContext access,
        CancellationToken cancellationToken = default)
    {
        if (!access.IsAuthenticated)
        {
            return ServiceResult<GradeDto>.Failure("Authentication is required.", ServiceErrorType.Unauthorized);
        }

        if (!access.IsAdmin)
        {
            return ServiceResult<GradeDto>.Failure("Only administrators can edit grades.", ServiceErrorType.Forbidden);
        }

        var errors = AcademicValidator.ValidateGrade(request);
        if (errors.Count > 0)
        {
            return ServiceResult<GradeDto>.Failure(errors, ServiceErrorType.Validation);
        }

        var existing = await _academicRepository.GetGradeByIdAsync(gradeId, cancellationToken);
        if (existing is null)
        {
            return ServiceResult<GradeDto>.Failure($"Grade with ID {gradeId} was not found.", ServiceErrorType.NotFound);
        }

        existing.StudentId = request.StudentId;
        existing.CourseId = request.CourseId;
        existing.AssessmentTitle = request.AssessmentTitle.Trim();
        existing.AssessmentType = string.IsNullOrWhiteSpace(request.AssessmentType) ? "Exam" : request.AssessmentType.Trim();
        existing.Score = Math.Round(request.Score, 2);
        existing.Weight = request.Weight;
        existing.Feedback = string.IsNullOrWhiteSpace(request.Feedback) ? null : request.Feedback.Trim();

        await _academicRepository.UpdateGradeAsync(existing, cancellationToken);
        var reloaded = await _academicRepository.GetGradeByIdAsync(existing.Id, cancellationToken) ?? existing;
        return ServiceResult<GradeDto>.Success(MapGradeDto(reloaded), "Grade updated successfully.");
    }

    public async Task<ServiceResult> DeleteGradeAsync(
        int gradeId,
        AccessContext access,
        CancellationToken cancellationToken = default)
    {
        if (!access.IsAuthenticated)
        {
            return ServiceResult.Failure("Authentication is required.", ServiceErrorType.Unauthorized);
        }

        if (!access.IsAdmin)
        {
            return ServiceResult.Failure("Only administrators can delete grades.", ServiceErrorType.Forbidden);
        }

        var existing = await _academicRepository.GetGradeByIdAsync(gradeId, cancellationToken);
        if (existing is null)
        {
            return ServiceResult.Failure($"Grade with ID {gradeId} was not found.", ServiceErrorType.NotFound);
        }

        await _academicRepository.DeleteGradeAsync(existing, cancellationToken);
        return ServiceResult.Success("Grade deleted.");
    }

    public async Task<ServiceResult<StudentGpaSummaryDto>> GetStudentGpaSummaryAsync(
        int studentId,
        AccessContext access,
        CancellationToken cancellationToken = default)
    {
        if (!access.IsAuthenticated)
        {
            return ServiceResult<StudentGpaSummaryDto>.Failure("Authentication is required.", ServiceErrorType.Unauthorized);
        }

        if (!access.CanAccessStudent(studentId))
        {
            return ServiceResult<StudentGpaSummaryDto>.Failure(
                "You do not have permission to view another student's GPA and academic performance.",
                ServiceErrorType.Forbidden);
        }

        var student = await _studentRepository.GetByIdAsync(studentId, cancellationToken);
        if (student is null)
        {
            return ServiceResult<StudentGpaSummaryDto>.Failure($"Student with ID {studentId} was not found.", ServiceErrorType.NotFound);
        }

        var grades = await _academicRepository.GetGradesAsync(studentId, null, cancellationToken);
        var summary = _gpaCalculator.CalculateStudentGpa(student, grades);
        return ServiceResult<StudentGpaSummaryDto>.Success(summary);
    }

    // =========================================================================
    // SCHEDULE
    // =========================================================================

    public async Task<ServiceResult<IReadOnlyList<ScheduleEntryDto>>> GetScheduleAsync(
        int? studentId,
        int? courseId,
        AccessContext access,
        CancellationToken cancellationToken = default)
    {
        if (!access.IsAuthenticated)
        {
            return ServiceResult<IReadOnlyList<ScheduleEntryDto>>.Failure("Authentication is required.", ServiceErrorType.Unauthorized);
        }

        if (studentId.HasValue)
        {
            if (!access.CanAccessStudent(studentId.Value))
            {
                return ServiceResult<IReadOnlyList<ScheduleEntryDto>>.Failure(
                    "You do not have permission to view another student's schedule.",
                    ServiceErrorType.Forbidden);
            }

            var enrollments = await _academicRepository.GetEnrollmentsAsync(studentId.Value, null, cancellationToken);
            var activeCourseIds = enrollments
                .Where(e => e.Status == EnrollmentStatus.Enrolled)
                .Select(e => e.CourseId)
                .Distinct()
                .ToList();

            if (activeCourseIds.Count == 0)
            {
                return ServiceResult<IReadOnlyList<ScheduleEntryDto>>.Success([]);
            }

            var studentEntries = await _academicRepository.GetScheduleEntriesAsync(courseId, activeCourseIds, cancellationToken);
            return ServiceResult<IReadOnlyList<ScheduleEntryDto>>.Success(studentEntries.Select(MapScheduleDto).ToList());
        }

        var entries = await _academicRepository.GetScheduleEntriesAsync(courseId, null, cancellationToken);
        return ServiceResult<IReadOnlyList<ScheduleEntryDto>>.Success(entries.Select(MapScheduleDto).ToList());
    }

    public async Task<ServiceResult<IReadOnlyList<ScheduleEntryDto>>> GetUpcomingClassesAsync(
        int studentId,
        DateTime referenceLocalTime,
        int maxCount,
        AccessContext access,
        CancellationToken cancellationToken = default)
    {
        var scheduleResult = await GetScheduleAsync(studentId, null, access, cancellationToken);
        if (!scheduleResult.Succeeded || scheduleResult.Data is null)
        {
            return scheduleResult;
        }

        var currentDay = (int)referenceLocalTime.DayOfWeek;
        var currentTime = TimeOnly.FromDateTime(referenceLocalTime);

        var ordered = scheduleResult.Data
            .OrderBy(entry =>
            {
                var entryDay = (int)entry.DayOfWeek;
                var daysAhead = (entryDay - currentDay + 7) % 7;
                if (daysAhead == 0 && entry.EndTime < currentTime)
                {
                    daysAhead = 7;
                }
                return daysAhead;
            })
            .ThenBy(entry => entry.StartTime)
            .Take(Math.Max(1, maxCount))
            .ToList();

        return ServiceResult<IReadOnlyList<ScheduleEntryDto>>.Success(ordered);
    }

    public async Task<ServiceResult<ScheduleEntryDto>> CreateScheduleEntryAsync(
        SaveScheduleEntryRequestDto request,
        AccessContext access,
        CancellationToken cancellationToken = default)
    {
        if (!access.IsAuthenticated)
        {
            return ServiceResult<ScheduleEntryDto>.Failure("Authentication is required.", ServiceErrorType.Unauthorized);
        }

        if (!access.IsAdmin)
        {
            return ServiceResult<ScheduleEntryDto>.Failure("Only administrators can manage class schedules.", ServiceErrorType.Forbidden);
        }

        var errors = AcademicValidator.ValidateScheduleEntry(request);
        if (errors.Count > 0)
        {
            return ServiceResult<ScheduleEntryDto>.Failure(errors, ServiceErrorType.Validation);
        }

        var course = await _academicRepository.GetCourseByIdAsync(request.CourseId, cancellationToken);
        if (course is null)
        {
            return ServiceResult<ScheduleEntryDto>.Failure("Selected course does not exist.", ServiceErrorType.NotFound);
        }

        var entry = new ScheduleEntry
        {
            CourseId = request.CourseId,
            DayOfWeek = request.DayOfWeek,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Classroom = request.Classroom.Trim(),
            LessonType = request.LessonType.Trim()
        };

        var created = await _academicRepository.AddScheduleEntryAsync(entry, cancellationToken);
        var loaded = await _academicRepository.GetScheduleEntryByIdAsync(created.Id, cancellationToken) ?? created;
        return ServiceResult<ScheduleEntryDto>.Success(MapScheduleDto(loaded), "Schedule entry added.");
    }

    public async Task<ServiceResult<ScheduleEntryDto>> UpdateScheduleEntryAsync(
        int id,
        SaveScheduleEntryRequestDto request,
        AccessContext access,
        CancellationToken cancellationToken = default)
    {
        if (!access.IsAuthenticated)
        {
            return ServiceResult<ScheduleEntryDto>.Failure("Authentication is required.", ServiceErrorType.Unauthorized);
        }

        if (!access.IsAdmin)
        {
            return ServiceResult<ScheduleEntryDto>.Failure("Only administrators can update class schedules.", ServiceErrorType.Forbidden);
        }

        var errors = AcademicValidator.ValidateScheduleEntry(request);
        if (errors.Count > 0)
        {
            return ServiceResult<ScheduleEntryDto>.Failure(errors, ServiceErrorType.Validation);
        }

        var existing = await _academicRepository.GetScheduleEntryByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return ServiceResult<ScheduleEntryDto>.Failure("Schedule entry was not found.", ServiceErrorType.NotFound);
        }

        existing.CourseId = request.CourseId;
        existing.DayOfWeek = request.DayOfWeek;
        existing.StartTime = request.StartTime;
        existing.EndTime = request.EndTime;
        existing.Classroom = request.Classroom.Trim();
        existing.LessonType = request.LessonType.Trim();

        await _academicRepository.UpdateScheduleEntryAsync(existing, cancellationToken);
        var reloaded = await _academicRepository.GetScheduleEntryByIdAsync(existing.Id, cancellationToken) ?? existing;
        return ServiceResult<ScheduleEntryDto>.Success(MapScheduleDto(reloaded), "Schedule entry updated.");
    }

    public async Task<ServiceResult> DeleteScheduleEntryAsync(
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
            return ServiceResult.Failure("Only administrators can delete schedule entries.", ServiceErrorType.Forbidden);
        }

        var existing = await _academicRepository.GetScheduleEntryByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return ServiceResult.Failure("Schedule entry was not found.", ServiceErrorType.NotFound);
        }

        await _academicRepository.DeleteScheduleEntryAsync(existing, cancellationToken);
        return ServiceResult.Success("Schedule entry deleted.");
    }

    // =========================================================================
    // ATTENDANCE
    // =========================================================================

    public async Task<ServiceResult<StudentAttendanceStatsDto>> GetStudentAttendanceStatsAsync(
        int studentId,
        AccessContext access,
        CancellationToken cancellationToken = default)
    {
        if (!access.IsAuthenticated)
        {
            return ServiceResult<StudentAttendanceStatsDto>.Failure("Authentication is required.", ServiceErrorType.Unauthorized);
        }

        if (!access.CanAccessStudent(studentId))
        {
            return ServiceResult<StudentAttendanceStatsDto>.Failure(
                "You do not have permission to view another student's attendance statistics.",
                ServiceErrorType.Forbidden);
        }

        var student = await _studentRepository.GetByIdAsync(studentId, cancellationToken);
        if (student is null)
        {
            return ServiceResult<StudentAttendanceStatsDto>.Failure($"Student with ID {studentId} was not found.", ServiceErrorType.NotFound);
        }

        var records = await _academicRepository.GetAttendanceRecordsAsync(studentId, null, cancellationToken);
        var recordDtos = records.Select(MapAttendanceDto).ToList();

        var total = recordDtos.Count;
        var present = recordDtos.Count(r => r.Status == AttendanceStatus.Present);
        var late = recordDtos.Count(r => r.Status == AttendanceStatus.Late);
        var absent = recordDtos.Count(r => r.Status == AttendanceStatus.Absent);

        var attendanceRate = total > 0
            ? Math.Round((present + late) * 100.0m / total, 1)
            : 0.0m;

        var strictPresentRate = total > 0
            ? Math.Round(present * 100.0m / total, 1)
            : 0.0m;

        var breakdowns = recordDtos
            .GroupBy(r => new { r.CourseId, r.CourseCode, r.CourseTitle })
            .Select(g =>
            {
                var cTotal = g.Count();
                var cPresent = g.Count(x => x.Status == AttendanceStatus.Present);
                var cLate = g.Count(x => x.Status == AttendanceStatus.Late);
                var cAbsent = g.Count(x => x.Status == AttendanceStatus.Absent);
                var cRate = cTotal > 0 ? Math.Round((cPresent + cLate) * 100.0m / cTotal, 1) : 0.0m;

                return new CourseAttendanceSummaryDto(
                    g.Key.CourseId,
                    g.Key.CourseCode,
                    g.Key.CourseTitle,
                    cTotal,
                    cPresent,
                    cLate,
                    cAbsent,
                    cRate);
            })
            .OrderBy(c => c.CourseCode)
            .ToList();

        var stats = new StudentAttendanceStatsDto(
            StudentId: studentId,
            TotalSessions: total,
            PresentCount: present,
            LateCount: late,
            AbsentCount: absent,
            AttendanceRatePercentage: attendanceRate,
            StrictPresentPercentage: strictPresentRate,
            CourseBreakdowns: breakdowns,
            Records: recordDtos);

        return ServiceResult<StudentAttendanceStatsDto>.Success(stats);
    }

    public async Task<ServiceResult<IReadOnlyList<AttendanceRecordDto>>> GetAttendanceRecordsAsync(
        int? studentId,
        int? courseId,
        AccessContext access,
        CancellationToken cancellationToken = default)
    {
        if (!access.IsAuthenticated)
        {
            return ServiceResult<IReadOnlyList<AttendanceRecordDto>>.Failure("Authentication is required.", ServiceErrorType.Unauthorized);
        }

        if (!access.IsAdmin)
        {
            var effectiveStudentId = studentId ?? access.StudentId;
            if (!effectiveStudentId.HasValue || !access.CanAccessStudent(effectiveStudentId.Value))
            {
                return ServiceResult<IReadOnlyList<AttendanceRecordDto>>.Failure(
                    "Students may only view their own attendance records.",
                    ServiceErrorType.Forbidden);
            }
            studentId = effectiveStudentId.Value;
        }

        var records = await _academicRepository.GetAttendanceRecordsAsync(studentId, courseId, cancellationToken);
        return ServiceResult<IReadOnlyList<AttendanceRecordDto>>.Success(records.Select(MapAttendanceDto).ToList());
    }

    public async Task<ServiceResult<AttendanceRecordDto>> RecordAttendanceAsync(
        SaveAttendanceRequestDto request,
        AccessContext access,
        CancellationToken cancellationToken = default)
    {
        if (!access.IsAuthenticated)
        {
            return ServiceResult<AttendanceRecordDto>.Failure("Authentication is required.", ServiceErrorType.Unauthorized);
        }

        if (!access.IsAdmin)
        {
            return ServiceResult<AttendanceRecordDto>.Failure("Only administrators can record attendance.", ServiceErrorType.Forbidden);
        }

        var errors = AcademicValidator.ValidateAttendance(request);
        if (errors.Count > 0)
        {
            return ServiceResult<AttendanceRecordDto>.Failure(errors, ServiceErrorType.Validation);
        }

        var student = await _studentRepository.GetByIdAsync(request.StudentId, cancellationToken);
        if (student is null)
        {
            return ServiceResult<AttendanceRecordDto>.Failure("Selected student does not exist.", ServiceErrorType.NotFound);
        }

        var course = await _academicRepository.GetCourseByIdAsync(request.CourseId, cancellationToken);
        if (course is null)
        {
            return ServiceResult<AttendanceRecordDto>.Failure("Selected course does not exist.", ServiceErrorType.NotFound);
        }

        var record = new AttendanceRecord
        {
            StudentId = request.StudentId,
            CourseId = request.CourseId,
            Date = request.Date,
            Status = request.Status,
            Topic = string.IsNullOrWhiteSpace(request.Topic) ? null : request.Topic.Trim(),
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            RecordedAt = DateTime.UtcNow
        };

        var created = await _academicRepository.AddAttendanceAsync(record, cancellationToken);
        var loaded = await _academicRepository.GetAttendanceByIdAsync(created.Id, cancellationToken) ?? created;
        return ServiceResult<AttendanceRecordDto>.Success(MapAttendanceDto(loaded), "Attendance recorded.");
    }

    public async Task<ServiceResult<AttendanceRecordDto>> UpdateAttendanceAsync(
        int id,
        SaveAttendanceRequestDto request,
        AccessContext access,
        CancellationToken cancellationToken = default)
    {
        if (!access.IsAuthenticated)
        {
            return ServiceResult<AttendanceRecordDto>.Failure("Authentication is required.", ServiceErrorType.Unauthorized);
        }

        if (!access.IsAdmin)
        {
            return ServiceResult<AttendanceRecordDto>.Failure("Only administrators can update attendance records.", ServiceErrorType.Forbidden);
        }

        var errors = AcademicValidator.ValidateAttendance(request);
        if (errors.Count > 0)
        {
            return ServiceResult<AttendanceRecordDto>.Failure(errors, ServiceErrorType.Validation);
        }

        var existing = await _academicRepository.GetAttendanceByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return ServiceResult<AttendanceRecordDto>.Failure("Attendance record was not found.", ServiceErrorType.NotFound);
        }

        existing.StudentId = request.StudentId;
        existing.CourseId = request.CourseId;
        existing.Date = request.Date;
        existing.Status = request.Status;
        existing.Topic = string.IsNullOrWhiteSpace(request.Topic) ? null : request.Topic.Trim();
        existing.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();

        await _academicRepository.UpdateAttendanceAsync(existing, cancellationToken);
        var reloaded = await _academicRepository.GetAttendanceByIdAsync(existing.Id, cancellationToken) ?? existing;
        return ServiceResult<AttendanceRecordDto>.Success(MapAttendanceDto(reloaded), "Attendance record updated.");
    }

    public async Task<ServiceResult> DeleteAttendanceAsync(
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
            return ServiceResult.Failure("Only administrators can delete attendance records.", ServiceErrorType.Forbidden);
        }

        var existing = await _academicRepository.GetAttendanceByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return ServiceResult.Failure("Attendance record was not found.", ServiceErrorType.NotFound);
        }

        await _academicRepository.DeleteAttendanceAsync(existing, cancellationToken);
        return ServiceResult.Success("Attendance record deleted.");
    }

    // =========================================================================
    // MAPPING HELPERS
    // =========================================================================

    private static TeacherDto MapTeacherDto(Teacher t) => new(
        t.Id,
        t.FullName,
        t.Email,
        t.Department,
        t.AcademicTitle,
        t.OfficeRoom,
        t.PhoneNumber,
        t.Bio,
        t.Courses?.Count ?? 0,
        t.Courses?.Select(c => c.CourseCode).OrderBy(c => c).ToList() ?? []);

    private static CourseDto MapCourseDto(Course c) => new(
        c.Id,
        c.CourseCode,
        c.Title,
        c.Description,
        c.Credits,
        c.Department,
        c.Semester,
        c.TeacherId,
        c.Teacher?.FullName,
        c.Teacher?.AcademicTitle,
        c.IsActive,
        c.Enrollments?.Count(e => e.Status == EnrollmentStatus.Enrolled) ?? 0);

    private static EnrollmentDto MapEnrollmentDto(Enrollment e) => new(
        e.Id,
        e.StudentId,
        e.Student?.FullName ?? $"Student #{e.StudentId}",
        e.Student?.StudentNumber ?? $"ID-{e.StudentId}",
        e.CourseId,
        e.Course?.CourseCode ?? $"CRS-{e.CourseId}",
        e.Course?.Title ?? $"Course #{e.CourseId}",
        e.Course?.Credits ?? 3,
        e.Course?.Teacher?.FullName,
        e.Semester,
        e.Status,
        e.EnrolledAt);

    private GradeDto MapGradeDto(Grade g)
    {
        var (letter, points) = _gpaCalculator.EvaluatePercentage(g.Score);
        return new GradeDto(
            g.Id,
            g.StudentId,
            g.Student?.FullName ?? $"Student #{g.StudentId}",
            g.Student?.StudentNumber ?? $"ID-{g.StudentId}",
            g.CourseId,
            g.Course?.CourseCode ?? $"CRS-{g.CourseId}",
            g.Course?.Title ?? $"Course #{g.CourseId}",
            g.Course?.Credits ?? 3,
            g.AssessmentTitle,
            g.AssessmentType,
            Math.Round(g.Score, 2),
            g.Weight,
            letter,
            points,
            g.Feedback,
            g.GradedAt);
    }

    private static ScheduleEntryDto MapScheduleDto(ScheduleEntry s) => new(
        s.Id,
        s.CourseId,
        s.Course?.CourseCode ?? $"CRS-{s.CourseId}",
        s.Course?.Title ?? $"Course #{s.CourseId}",
        s.Course?.Teacher?.FullName,
        s.DayOfWeek,
        s.StartTime,
        s.EndTime,
        s.Classroom,
        s.LessonType);

    private static AttendanceRecordDto MapAttendanceDto(AttendanceRecord a) => new(
        a.Id,
        a.StudentId,
        a.Student?.FullName ?? $"Student #{a.StudentId}",
        a.Student?.StudentNumber ?? $"ID-{a.StudentId}",
        a.CourseId,
        a.Course?.CourseCode ?? $"CRS-{a.CourseId}",
        a.Course?.Title ?? $"Course #{a.CourseId}",
        a.Date,
        a.Status,
        a.Topic,
        a.Notes,
        a.RecordedAt);
}
