using CRM_System.Core.Students.Models;

namespace CRM_System.Core.Academic.Models;

public class AttendanceRecord
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
    public string? Topic { get; set; }
    public string? Notes { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    public Student? Student { get; set; }
    public Course? Course { get; set; }
}
