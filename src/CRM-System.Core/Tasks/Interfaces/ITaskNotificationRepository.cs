using CRM_System.Core.Tasks.Models;

namespace CRM_System.Core.Tasks.Interfaces;

public interface ITaskNotificationRepository
{
    // Tasks
    Task<IReadOnlyList<StudentTask>> GetTasksAsync(int? studentId, int? courseId, CancellationToken cancellationToken = default);
    Task<StudentTask?> GetTaskByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<StudentTask> AddTaskAsync(StudentTask task, CancellationToken cancellationToken = default);
    Task UpdateTaskAsync(StudentTask task, CancellationToken cancellationToken = default);
    Task DeleteTaskAsync(StudentTask task, CancellationToken cancellationToken = default);

    // Notifications
    Task<IReadOnlyList<Notification>> GetNotificationsAsync(int? studentId, int? userId, bool unreadOnly, CancellationToken cancellationToken = default);
    Task<Notification?> GetNotificationByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> NotificationExistsForTaskAsync(int studentId, int taskId, NotificationType type, CancellationToken cancellationToken = default);
    Task<Notification> AddNotificationAsync(Notification notification, CancellationToken cancellationToken = default);
    Task UpdateNotificationAsync(Notification notification, CancellationToken cancellationToken = default);
    Task<int> MarkAllAsReadAsync(int? studentId, int? userId, DateTime readAtUtc, CancellationToken cancellationToken = default);
    Task DeleteNotificationAsync(Notification notification, CancellationToken cancellationToken = default);
}
