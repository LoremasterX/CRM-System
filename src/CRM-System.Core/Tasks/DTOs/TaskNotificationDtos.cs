using System.ComponentModel.DataAnnotations;
using CRM_System.Core.Tasks.Models;

namespace CRM_System.Core.Tasks.DTOs;

public sealed record StudentTaskDto(
    int Id,
    int StudentId,
    string StudentName,
    string StudentNumber,
    int? CourseId,
    string? CourseCode,
    string? CourseTitle,
    string Title,
    string Description,
    DateTime DueDate,
    TaskPriority Priority,
    StudentTaskStatus Status,
    bool IsOverdue,
    bool IsDueSoon,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? CompletedAt);

public class SaveStudentTaskRequestDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Valid student is required.")]
    public int StudentId { get; set; }

    public int? CourseId { get; set; }

    [Required(ErrorMessage = "Task title is required.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "Task title must be between 3 and 150 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Task description is required.")]
    [StringLength(1000, MinimumLength = 3, ErrorMessage = "Task description must be between 3 and 1000 characters.")]
    public string Description { get; set; } = string.Empty;

    public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(3);

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public StudentTaskStatus Status { get; set; } = StudentTaskStatus.Pending;
}

public class TaskFilterDto
{
    public int? StudentId { get; set; }
    public int? CourseId { get; set; }
    public StudentTaskStatus? Status { get; set; }
    public TaskPriority? Priority { get; set; }
    public string? SearchTerm { get; set; }
    public string SortBy { get; set; } = "deadline_asc";
}

public sealed record TaskSummaryStatsDto(
    int TotalTasks,
    int PendingCount,
    int InProgressCount,
    int CompletedCount,
    int OverdueCount,
    int DueSoonCount,
    int HighPriorityActiveCount,
    decimal CompletionRatePercentage);

public sealed record CalendarDayEventDto(
    DateOnly Date,
    string EventCategory,
    string Title,
    string Subtitle,
    string? CourseCode,
    string BadgeText,
    string BadgeVariant,
    int? TaskId,
    bool IsCompleted,
    bool IsOverdue);

public sealed record AcademicCalendarMonthDto(
    int Year,
    int Month,
    string MonthTitle,
    IReadOnlyList<CalendarDayEventDto> Events,
    IReadOnlyList<StudentTaskDto> UpcomingDeadlines,
    IReadOnlyList<StudentTaskDto> OverdueTasks);

public sealed record NotificationDto(
    int Id,
    int? UserId,
    int? StudentId,
    string? StudentName,
    int? RelatedTaskId,
    string? RelatedTaskTitle,
    int? RelatedCourseId,
    string? RelatedCourseCode,
    string Title,
    string Message,
    NotificationType Type,
    bool IsRead,
    DateTime CreatedAt,
    DateTime? ReadAt);

public class CreateNotificationRequestDto
{
    public int? StudentId { get; set; }
    public int? UserId { get; set; }
    public int? RelatedTaskId { get; set; }
    public int? RelatedCourseId { get; set; }

    [Required(ErrorMessage = "Notification title is required.")]
    [StringLength(150, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Notification message is required.")]
    [StringLength(1000, MinimumLength = 3)]
    public string Message { get; set; } = string.Empty;

    public NotificationType Type { get; set; } = NotificationType.SystemAnnouncement;
}
