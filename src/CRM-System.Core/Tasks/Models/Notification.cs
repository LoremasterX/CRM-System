using CRM_System.Core.Academic.Models;
using CRM_System.Core.Students.Models;

namespace CRM_System.Core.Tasks.Models;

public class Notification
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public ApplicationUser? User { get; set; }
    public int? StudentId { get; set; }
    public Student? Student { get; set; }
    public int? RelatedTaskId { get; set; }
    public StudentTask? RelatedTask { get; set; }
    public int? RelatedCourseId { get; set; }
    public Course? RelatedCourse { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; } = NotificationType.DeadlineReminder;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
}
