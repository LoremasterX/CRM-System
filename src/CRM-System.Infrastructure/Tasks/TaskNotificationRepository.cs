using CRM_System.Core.Tasks.Interfaces;
using CRM_System.Core.Tasks.Models;
using CRM_System.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CRM_System.Infrastructure.Tasks;

public class TaskNotificationRepository : ITaskNotificationRepository
{
    private readonly CrmDbContext _db;

    public TaskNotificationRepository(CrmDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<StudentTask>> GetTasksAsync(
        int? studentId,
        int? courseId,
        CancellationToken cancellationToken = default)
    {
        var query = _db.StudentTasks
            .AsNoTracking()
            .Include(t => t.Student)
            .Include(t => t.Course)
            .AsQueryable();

        if (studentId.HasValue && studentId.Value > 0)
        {
            query = query.Where(t => t.StudentId == studentId.Value);
        }

        if (courseId.HasValue && courseId.Value > 0)
        {
            query = query.Where(t => t.CourseId == courseId.Value);
        }

        return await query
            .OrderBy(t => t.DueDate)
            .ThenByDescending(t => t.Priority)
            .ToListAsync(cancellationToken);
    }

    public async Task<StudentTask?> GetTaskByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _db.StudentTasks
            .Include(t => t.Student)
            .Include(t => t.Course)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<StudentTask> AddTaskAsync(StudentTask task, CancellationToken cancellationToken = default)
    {
        _db.StudentTasks.Add(task);
        await _db.SaveChangesAsync(cancellationToken);
        return task;
    }

    public async Task UpdateTaskAsync(StudentTask task, CancellationToken cancellationToken = default)
    {
        _db.StudentTasks.Update(task);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteTaskAsync(StudentTask task, CancellationToken cancellationToken = default)
    {
        var relatedNotifications = await _db.Notifications
            .Where(n => n.RelatedTaskId == task.Id)
            .ToListAsync(cancellationToken);

        foreach (var n in relatedNotifications)
        {
            n.RelatedTaskId = null;
        }

        _db.StudentTasks.Remove(task);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Notification>> GetNotificationsAsync(
        int? studentId,
        int? userId,
        bool unreadOnly,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Notifications
            .AsNoTracking()
            .Include(n => n.Student)
            .Include(n => n.RelatedTask)
            .Include(n => n.RelatedCourse)
            .AsQueryable();

        if (studentId.HasValue && studentId.Value > 0)
        {
            if (userId.HasValue && userId.Value > 0)
            {
                query = query.Where(n => n.StudentId == studentId.Value || n.UserId == userId.Value);
            }
            else
            {
                query = query.Where(n => n.StudentId == studentId.Value);
            }
        }
        else if (userId.HasValue && userId.Value > 0)
        {
            query = query.Where(n => n.UserId == userId.Value);
        }

        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .ThenByDescending(n => n.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<Notification?> GetNotificationByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _db.Notifications
            .Include(n => n.Student)
            .Include(n => n.RelatedTask)
            .Include(n => n.RelatedCourse)
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
    }

    public async Task<bool> NotificationExistsForTaskAsync(
        int studentId,
        int taskId,
        NotificationType type,
        CancellationToken cancellationToken = default)
    {
        return await _db.Notifications
            .AnyAsync(n => n.StudentId == studentId && n.RelatedTaskId == taskId && n.Type == type, cancellationToken);
    }

    public async Task<Notification> AddNotificationAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync(cancellationToken);
        return notification;
    }

    public async Task UpdateNotificationAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        _db.Notifications.Update(notification);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> MarkAllAsReadAsync(
        int? studentId,
        int? userId,
        DateTime readAtUtc,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Notifications.Where(n => !n.IsRead);

        if (studentId.HasValue && studentId.Value > 0)
        {
            if (userId.HasValue && userId.Value > 0)
            {
                query = query.Where(n => n.StudentId == studentId.Value || n.UserId == userId.Value);
            }
            else
            {
                query = query.Where(n => n.StudentId == studentId.Value);
            }
        }
        else if (userId.HasValue && userId.Value > 0)
        {
            query = query.Where(n => n.UserId == userId.Value);
        }

        var unread = await query.ToListAsync(cancellationToken);
        foreach (var item in unread)
        {
            item.IsRead = true;
            item.ReadAt = readAtUtc;
        }

        if (unread.Count > 0)
        {
            await _db.SaveChangesAsync(cancellationToken);
        }

        return unread.Count;
    }

    public async Task DeleteNotificationAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        _db.Notifications.Remove(notification);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
