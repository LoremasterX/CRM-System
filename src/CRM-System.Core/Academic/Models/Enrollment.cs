using CRM_System.Core.Students.Models;

namespace CRM_System.Core.Academic.Models;

public class Enrollment
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public string Semester { get; set; } = "Fall 2026";
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Enrolled;
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

    public Student? Student { get; set; }
    public Course? Course { get; set; }
}
