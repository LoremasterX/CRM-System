using CRM_System.Core.Common;
using CRM_System.Core.Tasks.DTOs;
using CRM_System.Core.Tasks.Models;

namespace CRM_System.Core.Tasks.Interfaces;

public interface ITaskNotificationService
{
    // Tasks CRUD, Filtering & Sorting
    Task<ServiceResult<IReadOnlyList<StudentTaskDto>>> GetTasksAsync(TaskFilterDto? filter, AccessContext access, DateTime? referenceUtcNow = null, CancellationToken cancellationToken = default);
    Task<ServiceResult<StudentTaskDto>> GetTaskByIdAsync(int taskId, AccessContext access, DateTime? referenceUtcNow = null, CancellationToken cancellationToken = default);
    Task<ServiceResult<TaskSummaryStatsDto>> GetTaskSummaryStatsAsync(int? studentId, AccessContext access, DateTime? referenceUtcNow = null, CancellationToken cancellationToken = default);
    Task<ServiceResult<StudentTaskDto>> CreateTaskAsync(SaveStudentTaskRequestDto request, AccessContext access, DateTime? referenceUtcNow = null, CancellationToken cancellationToken = default);
    Task<ServiceResult<StudentTaskDto>> UpdateTaskAsync(int taskId, SaveStudentTaskRequestDto request, AccessContext access, DateTime? referenceUtcNow = null, CancellationToken cancellationToken = default);
    Task<ServiceResult<StudentTaskDto>> UpdateTaskStatusAsync(int taskId, StudentTaskStatus status, AccessContext access, DateTime? referenceUtcNow = null, CancellationToken cancellationToken = default);
    Task<ServiceResult> DeleteTaskAsync(int taskId, AccessContext access, CancellationToken cancellationToken = default);

    // Academic Calendar (Tasks + Weekly Schedule)
    Task<ServiceResult<AcademicCalendarMonthDto>> GetCalendarMonthAsync(int? studentId, int year, int month, AccessContext access, DateTime? referenceUtcNow = null, CancellationToken cancellationToken = default);

    // Notifications & Deadline Alert Generation
    Task<ServiceResult<IReadOnlyList<NotificationDto>>> GetNotificationsAsync(int? studentId, bool unreadOnly, AccessContext access, CancellationToken cancellationToken = default);
    Task<ServiceResult<int>> GetUnreadCountAsync(int? studentId, AccessContext access, CancellationToken cancellationToken = default);
    Task<ServiceResult<int>> SynchronizeDeadlineNotificationsAsync(int? studentId, AccessContext access, DateTime? referenceUtcNow = null, CancellationToken cancellationToken = default);
    Task<ServiceResult<NotificationDto>> CreateNotificationAsync(CreateNotificationRequestDto request, AccessContext access, CancellationToken cancellationToken = default);
    Task<ServiceResult<NotificationDto>> MarkNotificationReadStateAsync(int notificationId, bool isRead, AccessContext access, CancellationToken cancellationToken = default);
    Task<ServiceResult<int>> MarkAllNotificationsAsReadAsync(int? studentId, AccessContext access, CancellationToken cancellationToken = default);
    Task<ServiceResult> DeleteNotificationAsync(int notificationId, AccessContext access, CancellationToken cancellationToken = default);
}
