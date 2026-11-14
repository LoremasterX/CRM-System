using CRM_System.Core.Academic.Models;
using CRM_System.Core.Students.Models;

namespace CRM_System.Core.Tasks.Models;

public class StudentTask
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public Student? Student { get; set; }
    public int? CourseId { get; set; }
    public Course? Course { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public StudentTaskStatus Status { get; set; } = StudentTaskStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public StudentTaskStatus GetEffectiveStatus(DateTime utcNow)
    {
        if (Status == StudentTaskStatus.Completed)
        {
            return StudentTaskStatus.Completed;
        }

        if (DueDate < utcNow)
        {
            return StudentTaskStatus.Overdue;
        }

        return Status;
    }
}
