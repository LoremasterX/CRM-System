using CRM_System.Core.Academic.Models;
using CRM_System.Core.Students.Models;
using CRM_System.Core.Tasks.Models;

namespace CRM_System.Core.Dashboard.Interfaces;

public sealed record DashboardDatasetSnapshot(
    IReadOnlyList<Student> Students,
    IReadOnlyList<Teacher> Teachers,
    IReadOnlyList<Course> Courses,
    IReadOnlyList<Enrollment> Enrollments,
    IReadOnlyList<Grade> Grades,
    IReadOnlyList<AttendanceRecord> AttendanceRecords,
    IReadOnlyList<StudentTask> StudentTasks,
    IReadOnlyList<Notification> Notifications);

public interface IDashboardAnalyticsRepository
{
    Task<DashboardDatasetSnapshot> GetDatasetSnapshotAsync(
        int? scopedStudentId = null,
        CancellationToken cancellationToken = default);
}
