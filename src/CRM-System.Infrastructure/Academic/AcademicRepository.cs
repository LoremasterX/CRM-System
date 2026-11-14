using CRM_System.Core.Academic.Interfaces;
using CRM_System.Core.Academic.Models;
using CRM_System.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CRM_System.Infrastructure.Academic;

public class AcademicRepository : IAcademicRepository
{
    private readonly CrmDbContext _dbContext;

    public AcademicRepository(CrmDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Teachers
    public async Task<IReadOnlyList<Teacher>> GetTeachersAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Teachers
            .AsNoTracking()
            .Include(t => t.Courses)
            .OrderBy(t => t.FullName)
            .ToListAsync(cancellationToken);

    public Task<Teacher?> GetTeacherByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _dbContext.Teachers
            .Include(t => t.Courses)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public Task<bool> TeacherEmailExistsAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return _dbContext.Teachers.AnyAsync(
            t => t.Email.ToLower() == normalized && (!excludeId.HasValue || t.Id != excludeId.Value),
            cancellationToken);
    }

    public async Task<Teacher> AddTeacherAsync(Teacher teacher, CancellationToken cancellationToken = default)
    {
        _dbContext.Teachers.Add(teacher);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return teacher;
    }

    public async Task UpdateTeacherAsync(Teacher teacher, CancellationToken cancellationToken = default)
    {
        _dbContext.Teachers.Update(teacher);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteTeacherAsync(Teacher teacher, CancellationToken cancellationToken = default)
    {
        _dbContext.Teachers.Remove(teacher);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    // Courses
    public async Task<IReadOnlyList<Course>> GetCoursesAsync(
        string? searchTerm = null,
        string? department = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Course> query = _dbContext.Courses
            .AsNoTracking()
            .Include(c => c.Teacher)
            .Include(c => c.Enrollments);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLowerInvariant();
            query = query.Where(c =>
                c.CourseCode.ToLower().Contains(term) ||
                c.Title.ToLower().Contains(term) ||
                c.Description.ToLower().Contains(term) ||
                c.Department.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(department))
        {
            var dept = department.Trim();
            query = query.Where(c => c.Department == dept);
        }

        return await query
            .OrderBy(c => c.CourseCode)
            .ToListAsync(cancellationToken);
    }

    public Task<Course?> GetCourseByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _dbContext.Courses
            .Include(c => c.Teacher)
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<bool> CourseCodeExistsAsync(string courseCode, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var normalized = courseCode.Trim().ToUpperInvariant();
        return _dbContext.Courses.AnyAsync(
            c => c.CourseCode.ToUpper() == normalized && (!excludeId.HasValue || c.Id != excludeId.Value),
            cancellationToken);
    }

    public async Task<Course> AddCourseAsync(Course course, CancellationToken cancellationToken = default)
    {
        _dbContext.Courses.Add(course);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return course;
    }

    public async Task UpdateCourseAsync(Course course, CancellationToken cancellationToken = default)
    {
        _dbContext.Courses.Update(course);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteCourseAsync(Course course, CancellationToken cancellationToken = default)
    {
        _dbContext.Courses.Remove(course);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    // Enrollments
    public async Task<IReadOnlyList<Enrollment>> GetEnrollmentsAsync(
        int? studentId = null,
        int? courseId = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Enrollment> query = _dbContext.Enrollments
            .AsNoTracking()
            .Include(e => e.Student)
            .Include(e => e.Course)
                .ThenInclude(c => c!.Teacher);

        if (studentId.HasValue)
        {
            query = query.Where(e => e.StudentId == studentId.Value);
        }

        if (courseId.HasValue)
        {
            query = query.Where(e => e.CourseId == courseId.Value);
        }

        return await query
            .OrderBy(e => e.Course!.CourseCode)
            .ThenBy(e => e.Student!.FullName)
            .ToListAsync(cancellationToken);
    }

    public Task<Enrollment?> GetEnrollmentByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _dbContext.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Course)
                .ThenInclude(c => c!.Teacher)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public Task<Enrollment?> GetEnrollmentAsync(int studentId, int courseId, CancellationToken cancellationToken = default) =>
        _dbContext.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Course)
                .ThenInclude(c => c!.Teacher)
            .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId, cancellationToken);

    public async Task<Enrollment> AddEnrollmentAsync(Enrollment enrollment, CancellationToken cancellationToken = default)
    {
        _dbContext.Enrollments.Add(enrollment);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return enrollment;
    }

    public async Task UpdateEnrollmentAsync(Enrollment enrollment, CancellationToken cancellationToken = default)
    {
        _dbContext.Enrollments.Update(enrollment);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteEnrollmentAsync(Enrollment enrollment, CancellationToken cancellationToken = default)
    {
        _dbContext.Enrollments.Remove(enrollment);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    // Grades
    public async Task<IReadOnlyList<Grade>> GetGradesAsync(
        int? studentId = null,
        int? courseId = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Grade> query = _dbContext.Grades
            .AsNoTracking()
            .Include(g => g.Student)
            .Include(g => g.Course);

        if (studentId.HasValue)
        {
            query = query.Where(g => g.StudentId == studentId.Value);
        }

        if (courseId.HasValue)
        {
            query = query.Where(g => g.CourseId == courseId.Value);
        }

        return await query
            .OrderByDescending(g => g.GradedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<Grade?> GetGradeByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _dbContext.Grades
            .Include(g => g.Student)
            .Include(g => g.Course)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);

    public async Task<Grade> AddGradeAsync(Grade grade, CancellationToken cancellationToken = default)
    {
        _dbContext.Grades.Add(grade);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return grade;
    }

    public async Task UpdateGradeAsync(Grade grade, CancellationToken cancellationToken = default)
    {
        _dbContext.Grades.Update(grade);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteGradeAsync(Grade grade, CancellationToken cancellationToken = default)
    {
        _dbContext.Grades.Remove(grade);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    // Schedule
    public async Task<IReadOnlyList<ScheduleEntry>> GetScheduleEntriesAsync(
        int? courseId = null,
        IReadOnlyCollection<int>? courseIds = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<ScheduleEntry> query = _dbContext.ScheduleEntries
            .AsNoTracking()
            .Include(s => s.Course)
                .ThenInclude(c => c!.Teacher);

        if (courseId.HasValue)
        {
            query = query.Where(s => s.CourseId == courseId.Value);
        }

        if (courseIds is not null)
        {
            query = query.Where(s => courseIds.Contains(s.CourseId));
        }

        var list = await query.ToListAsync(cancellationToken);
        return list
            .OrderBy(s => s.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)s.DayOfWeek)
            .ThenBy(s => s.StartTime)
            .ToList();
    }

    public Task<ScheduleEntry?> GetScheduleEntryByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _dbContext.ScheduleEntries
            .Include(s => s.Course)
                .ThenInclude(c => c!.Teacher)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<ScheduleEntry> AddScheduleEntryAsync(ScheduleEntry entry, CancellationToken cancellationToken = default)
    {
        _dbContext.ScheduleEntries.Add(entry);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entry;
    }

    public async Task UpdateScheduleEntryAsync(ScheduleEntry entry, CancellationToken cancellationToken = default)
    {
        _dbContext.ScheduleEntries.Update(entry);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteScheduleEntryAsync(ScheduleEntry entry, CancellationToken cancellationToken = default)
    {
        _dbContext.ScheduleEntries.Remove(entry);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    // Attendance
    public async Task<IReadOnlyList<AttendanceRecord>> GetAttendanceRecordsAsync(
        int? studentId = null,
        int? courseId = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<AttendanceRecord> query = _dbContext.AttendanceRecords
            .AsNoTracking()
            .Include(a => a.Student)
            .Include(a => a.Course);

        if (studentId.HasValue)
        {
            query = query.Where(a => a.StudentId == studentId.Value);
        }

        if (courseId.HasValue)
        {
            query = query.Where(a => a.CourseId == courseId.Value);
        }

        return await query
            .OrderByDescending(a => a.Date)
            .ThenBy(a => a.Course!.CourseCode)
            .ToListAsync(cancellationToken);
    }

    public Task<AttendanceRecord?> GetAttendanceByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _dbContext.AttendanceRecords
            .Include(a => a.Student)
            .Include(a => a.Course)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<AttendanceRecord> AddAttendanceAsync(AttendanceRecord record, CancellationToken cancellationToken = default)
    {
        _dbContext.AttendanceRecords.Add(record);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return record;
    }

    public async Task UpdateAttendanceAsync(AttendanceRecord record, CancellationToken cancellationToken = default)
    {
        _dbContext.AttendanceRecords.Update(record);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAttendanceAsync(AttendanceRecord record, CancellationToken cancellationToken = default)
    {
        _dbContext.AttendanceRecords.Remove(record);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
