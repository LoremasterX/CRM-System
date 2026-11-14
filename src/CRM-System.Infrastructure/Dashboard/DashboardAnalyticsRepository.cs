using CRM_System.Core.Dashboard.Interfaces;
using CRM_System.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CRM_System.Infrastructure.Dashboard;

public class DashboardAnalyticsRepository : IDashboardAnalyticsRepository
{
    private readonly CrmDbContext _context;

    public DashboardAnalyticsRepository(CrmDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardDatasetSnapshot> GetDatasetSnapshotAsync(
        int? scopedStudentId = null,
        CancellationToken cancellationToken = default)
    {
        var studentsQuery = _context.Students.AsNoTracking();
        var enrollmentsQuery = _context.Enrollments
            .AsNoTracking()
            .Include(e => e.Student)
            .Include(e => e.Course)
                .ThenInclude(c => c!.Teacher)
            .AsQueryable();

        var gradesQuery = _context.Grades
            .AsNoTracking()
            .Include(g => g.Student)
            .Include(g => g.Course)
            .AsQueryable();

        var attendanceQuery = _context.AttendanceRecords
            .AsNoTracking()
            .Include(a => a.Student)
            .Include(a => a.Course)
            .AsQueryable();

        var tasksQuery = _context.StudentTasks
            .AsNoTracking()
            .Include(t => t.Student)
            .Include(t => t.Course)
            .AsQueryable();

        var notificationsQuery = _context.Notifications
            .AsNoTracking()
            .Include(n => n.Student)
            .Include(n => n.RelatedTask)
            .Include(n => n.RelatedCourse)
            .AsQueryable();

        if (scopedStudentId.HasValue)
        {
            var sid = scopedStudentId.Value;
            studentsQuery = studentsQuery.Where(s => s.Id == sid);
            enrollmentsQuery = enrollmentsQuery.Where(e => e.StudentId == sid);
            gradesQuery = gradesQuery.Where(g => g.StudentId == sid);
            attendanceQuery = attendanceQuery.Where(a => a.StudentId == sid);
            tasksQuery = tasksQuery.Where(t => t.StudentId == sid);
        }

        var students = await studentsQuery
            .OrderBy(s => s.FullName)
            .ToListAsync(cancellationToken);

        var teachers = await _context.Teachers
            .AsNoTracking()
            .OrderBy(t => t.FullName)
            .ToListAsync(cancellationToken);

        var courses = await _context.Courses
            .AsNoTracking()
            .Include(c => c.Teacher)
            .OrderBy(c => c.CourseCode)
            .ToListAsync(cancellationToken);

        var enrollments = await enrollmentsQuery
            .OrderByDescending(e => e.EnrolledAt)
            .ToListAsync(cancellationToken);

        var grades = await gradesQuery
            .OrderByDescending(g => g.GradedAt)
            .ToListAsync(cancellationToken);

        var attendance = await attendanceQuery
            .OrderByDescending(a => a.Date)
            .ToListAsync(cancellationToken);

        var tasks = await tasksQuery
            .OrderBy(t => t.DueDate)
            .ToListAsync(cancellationToken);

        var notifications = await notificationsQuery
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);

        return new DashboardDatasetSnapshot(
            Students: students,
            Teachers: teachers,
            Courses: courses,
            Enrollments: enrollments,
            Grades: grades,
            AttendanceRecords: attendance,
            StudentTasks: tasks,
            Notifications: notifications);
    }
}
