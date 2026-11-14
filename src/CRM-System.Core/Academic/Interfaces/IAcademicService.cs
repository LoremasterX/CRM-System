using CRM_System.Core.Academic.DTOs;
using CRM_System.Core.Academic.Models;
using CRM_System.Core.Common;

namespace CRM_System.Core.Academic.Interfaces;

public interface IAcademicService
{
    // Teachers
    Task<ServiceResult<IReadOnlyList<TeacherDto>>> GetTeachersAsync(AccessContext access, CancellationToken cancellationToken = default);
    Task<ServiceResult<TeacherDto>> CreateTeacherAsync(SaveTeacherRequestDto request, AccessContext access, CancellationToken cancellationToken = default);
    Task<ServiceResult<TeacherDto>> UpdateTeacherAsync(int id, SaveTeacherRequestDto request, AccessContext access, CancellationToken cancellationToken = default);
    Task<ServiceResult> DeleteTeacherAsync(int id, AccessContext access, CancellationToken cancellationToken = default);

    // Courses
    Task<ServiceResult<IReadOnlyList<CourseDto>>> GetCoursesAsync(string? searchTerm, string? department, AccessContext access, CancellationToken cancellationToken = default);
    Task<ServiceResult<CourseDto>> GetCourseByIdAsync(int id, AccessContext access, CancellationToken cancellationToken = default);
    Task<ServiceResult<CourseDto>> CreateCourseAsync(SaveCourseRequestDto request, AccessContext access, CancellationToken cancellationToken = default);
    Task<ServiceResult<CourseDto>> UpdateCourseAsync(int id, SaveCourseRequestDto request, AccessContext access, CancellationToken cancellationToken = default);
    Task<ServiceResult> DeleteCourseAsync(int id, AccessContext access, CancellationToken cancellationToken = default);

    // Enrollments
    Task<ServiceResult<IReadOnlyList<EnrollmentDto>>> GetEnrollmentsAsync(int? studentId, int? courseId, AccessContext access, CancellationToken cancellationToken = default);
    Task<ServiceResult<EnrollmentDto>> EnrollStudentAsync(CreateEnrollmentRequestDto request, AccessContext access, CancellationToken cancellationToken = default);
    Task<ServiceResult<EnrollmentDto>> UpdateEnrollmentStatusAsync(int enrollmentId, EnrollmentStatus status, AccessContext access, CancellationToken cancellationToken = default);
    Task<ServiceResult> RemoveEnrollmentAsync(int enrollmentId, AccessContext access, CancellationToken cancellationToken = default);

    // Grades & GPA
    Task<ServiceResult<IReadOnlyList<GradeDto>>> GetGradesAsync(int? studentId, int? courseId, AccessContext access, CancellationToken cancellationToken = default);
    Task<ServiceResult<GradeDto>> AddGradeAsync(SaveGradeRequestDto request, AccessContext access, CancellationToken cancellationToken = default);
    Task<ServiceResult<GradeDto>> UpdateGradeAsync(int gradeId, SaveGradeRequestDto request, AccessContext access, CancellationToken cancellationToken = default);
    Task<ServiceResult> DeleteGradeAsync(int gradeId, AccessContext access, CancellationToken cancellationToken = default);
    Task<ServiceResult<StudentGpaSummaryDto>> GetStudentGpaSummaryAsync(int studentId, AccessContext access, CancellationToken cancellationToken = default);

    // Schedule
    Task<ServiceResult<IReadOnlyList<ScheduleEntryDto>>> GetScheduleAsync(int? studentId, int? courseId, AccessContext access, CancellationToken cancellationToken = default);
    Task<ServiceResult<IReadOnlyList<ScheduleEntryDto>>> GetUpcomingClassesAsync(int studentId, DateTime referenceLocalTime, int maxCount, AccessContext access, CancellationToken cancellationToken = default);
    Task<ServiceResult<ScheduleEntryDto>> CreateScheduleEntryAsync(SaveScheduleEntryRequestDto request, AccessContext access, CancellationToken cancellationToken = default);
    Task<ServiceResult<ScheduleEntryDto>> UpdateScheduleEntryAsync(int id, SaveScheduleEntryRequestDto request, AccessContext access, CancellationToken cancellationToken = default);
    Task<ServiceResult> DeleteScheduleEntryAsync(int id, AccessContext access, CancellationToken cancellationToken = default);

    // Attendance
    Task<ServiceResult<StudentAttendanceStatsDto>> GetStudentAttendanceStatsAsync(int studentId, AccessContext access, CancellationToken cancellationToken = default);
    Task<ServiceResult<IReadOnlyList<AttendanceRecordDto>>> GetAttendanceRecordsAsync(int? studentId, int? courseId, AccessContext access, CancellationToken cancellationToken = default);
    Task<ServiceResult<AttendanceRecordDto>> RecordAttendanceAsync(SaveAttendanceRequestDto request, AccessContext access, CancellationToken cancellationToken = default);
    Task<ServiceResult<AttendanceRecordDto>> UpdateAttendanceAsync(int id, SaveAttendanceRequestDto request, AccessContext access, CancellationToken cancellationToken = default);
    Task<ServiceResult> DeleteAttendanceAsync(int id, AccessContext access, CancellationToken cancellationToken = default);
}
