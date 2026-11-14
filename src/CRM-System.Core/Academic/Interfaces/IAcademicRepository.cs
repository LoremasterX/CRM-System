using CRM_System.Core.Academic.Models;

namespace CRM_System.Core.Academic.Interfaces;

public interface IAcademicRepository
{
    // Teachers
    Task<IReadOnlyList<Teacher>> GetTeachersAsync(CancellationToken cancellationToken = default);
    Task<Teacher?> GetTeacherByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> TeacherEmailExistsAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default);
    Task<Teacher> AddTeacherAsync(Teacher teacher, CancellationToken cancellationToken = default);
    Task UpdateTeacherAsync(Teacher teacher, CancellationToken cancellationToken = default);
    Task DeleteTeacherAsync(Teacher teacher, CancellationToken cancellationToken = default);

    // Courses
    Task<IReadOnlyList<Course>> GetCoursesAsync(string? searchTerm = null, string? department = null, CancellationToken cancellationToken = default);
    Task<Course?> GetCourseByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> CourseCodeExistsAsync(string courseCode, int? excludeId = null, CancellationToken cancellationToken = default);
    Task<Course> AddCourseAsync(Course course, CancellationToken cancellationToken = default);
    Task UpdateCourseAsync(Course course, CancellationToken cancellationToken = default);
    Task DeleteCourseAsync(Course course, CancellationToken cancellationToken = default);

    // Enrollments
    Task<IReadOnlyList<Enrollment>> GetEnrollmentsAsync(int? studentId = null, int? courseId = null, CancellationToken cancellationToken = default);
    Task<Enrollment?> GetEnrollmentByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Enrollment?> GetEnrollmentAsync(int studentId, int courseId, CancellationToken cancellationToken = default);
    Task<Enrollment> AddEnrollmentAsync(Enrollment enrollment, CancellationToken cancellationToken = default);
    Task UpdateEnrollmentAsync(Enrollment enrollment, CancellationToken cancellationToken = default);
    Task DeleteEnrollmentAsync(Enrollment enrollment, CancellationToken cancellationToken = default);

    // Grades
    Task<IReadOnlyList<Grade>> GetGradesAsync(int? studentId = null, int? courseId = null, CancellationToken cancellationToken = default);
    Task<Grade?> GetGradeByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Grade> AddGradeAsync(Grade grade, CancellationToken cancellationToken = default);
    Task UpdateGradeAsync(Grade grade, CancellationToken cancellationToken = default);
    Task DeleteGradeAsync(Grade grade, CancellationToken cancellationToken = default);

    // Schedule
    Task<IReadOnlyList<ScheduleEntry>> GetScheduleEntriesAsync(int? courseId = null, IReadOnlyCollection<int>? courseIds = null, CancellationToken cancellationToken = default);
    Task<ScheduleEntry?> GetScheduleEntryByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ScheduleEntry> AddScheduleEntryAsync(ScheduleEntry entry, CancellationToken cancellationToken = default);
    Task UpdateScheduleEntryAsync(ScheduleEntry entry, CancellationToken cancellationToken = default);
    Task DeleteScheduleEntryAsync(ScheduleEntry entry, CancellationToken cancellationToken = default);

    // Attendance
    Task<IReadOnlyList<AttendanceRecord>> GetAttendanceRecordsAsync(int? studentId = null, int? courseId = null, CancellationToken cancellationToken = default);
    Task<AttendanceRecord?> GetAttendanceByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<AttendanceRecord> AddAttendanceAsync(AttendanceRecord record, CancellationToken cancellationToken = default);
    Task UpdateAttendanceAsync(AttendanceRecord record, CancellationToken cancellationToken = default);
    Task DeleteAttendanceAsync(AttendanceRecord record, CancellationToken cancellationToken = default);
}
