using CRM_System.Core.Students.Models;

namespace CRM_System.Core.Academic.Models;

public class Grade
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public string AssessmentTitle { get; set; } = string.Empty;
    public string AssessmentType { get; set; } = "Exam";
    public decimal Score { get; set; }
    public decimal Weight { get; set; } = 1.0m;
    public string? Feedback { get; set; }
    public DateTime GradedAt { get; set; } = DateTime.UtcNow;

    public Student? Student { get; set; }
    public Course? Course { get; set; }
}
