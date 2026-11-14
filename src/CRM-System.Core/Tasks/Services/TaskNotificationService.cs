using System.Globalization;
using CRM_System.Core.Academic.Interfaces;
using CRM_System.Core.Common;
using CRM_System.Core.Students.Interfaces;
using CRM_System.Core.Tasks.DTOs;
using CRM_System.Core.Tasks.Interfaces;
using CRM_System.Core.Tasks.Models;
using CRM_System.Core.Tasks.Validation;

namespace CRM_System.Core.Tasks.Services;

public class TaskNotificationService : ITaskNotificationService
{
    private readonly ITaskNotificationRepository _taskNotificationRepo;
    private readonly IStudentRepository _studentRepo;
    private readonly IAcademicRepository _academicRepo;

    public TaskNotificationService(
        ITaskNotificationRepository taskNotificationRepo,
        IStudentRepository studentRepo,
        IAcademicRepository academicRepo)
    {
        _taskNotificationRepo = taskNotificationRepo;
        _studentRepo = studentRepo;
        _academicRepo = academicRepo;
    }

    public async Task<ServiceResult<IReadOnlyList<StudentTaskDto>>> GetTasksAsync(
        TaskFilterDto? filter,
        AccessContext access,
        DateTime? referenceUtcNow = null,
        CancellationToken cancellationToken = default)
    {
        if (!access.IsAuthenticated)
        {
            return ServiceResult<IReadOnlyList<StudentTaskDto>>.Failure("Authentication is required.", ServiceErrorType.Unauthorized);
        }

        var now = referenceUtcNow ?? DateTime.UtcNow;
        var effectiveFilter = filter ?? new TaskFilterDto();

        int? targetStudentId = effectiveFilter.StudentId;
        if (!access.IsAdmin)
        {
            if (targetStudentId.HasValue && !access.CanAccessStudent(targetStudentId.Value))
            {
                return ServiceResult<IReadOnlyList<StudentTaskDto>>.Failure("Students may only view their own tasks.", ServiceErrorType.Forbidden);
            }

            targetStudentId = access.StudentId;
        }

        var tasks = await _taskNotificationRepo.GetTasksAsync(targetStudentId, effectiveFilter.CourseId, cancellationToken);
        var dtos = tasks.Select(t => MapTaskDto(t, now)).ToList();

        if (effectiveFilter.Status.HasValue)
        {
            dtos = dtos.Where(t => t.Status == effectiveFilter.Status.Value).ToList();
        }

        if (effectiveFilter.Priority.HasValue)
        {
            dtos = dtos.Where(t => t.Priority == effectiveFilter.Priority.Value).ToList();
        }

        if (!string.IsNullOrWhiteSpace(effectiveFilter.SearchTerm))
        {
            var term = effectiveFilter.SearchTerm.Trim();
            dtos = dtos.Where(t =>
                t.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                t.Description.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                (t.CourseCode?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (t.CourseTitle?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                t.StudentName.Contains(term, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        dtos = (effectiveFilter.SortBy?.ToLowerInvariant()) switch
        {
            "deadline_desc" => dtos.OrderByDescending(t => t.DueDate).ThenByDescending(t => t.Priority).ToList(),
            "priority_desc" => dtos.OrderByDescending(t => t.Priority).ThenBy(t => t.DueDate).ToList(),
            "priority_asc" => dtos.OrderBy(t => t.Priority).ThenBy(t => t.DueDate).ToList(),
            "status" => dtos.OrderBy(t => t.Status).ThenBy(t => t.DueDate).ToList(),
            _ => dtos.OrderBy(t => t.DueDate).ThenByDescending(t => t.Priority).ToList()
        };

        return ServiceResult<IReadOnlyList<StudentTaskDto>>.Success(dtos);
    }

    public async Task<ServiceResult<StudentTaskDto>> GetTaskByIdAsync(
        int taskId,
        AccessContext access,
        DateTime? referenceUtcNow = null,
        CancellationToken cancellationToken = default)
    {
        if (!access.IsAuthenticated)
        {
            return ServiceResult<StudentTaskDto>.Failure("Authentication is required.", ServiceErrorType.Unauthorized);
        }

        var task = await _taskNotificationRepo.GetTaskByIdAsync(taskId, cancellationToken);
        if (task is null)
        {
            return ServiceResult<StudentTaskDto>.Failure("Task not found.", ServiceErrorType.NotFound);
        }

        if (!access.CanAccessStudent(task.StudentId))
        {
            return ServiceResult<StudentTaskDto>.Failure("You do not have permission to view this task.", ServiceErrorType.Forbidden);
        }

        var now = referenceUtcNow ?? DateTime.UtcNow;
        return ServiceResult<StudentTaskDto>.Success(MapTaskDto(task, now));
    }

    public async Task<ServiceResult<TaskSummaryStatsDto>> GetTaskSummaryStatsAsync(
        int? studentId,
        AccessContext access,
        DateTime? referenceUtcNow = null,
        CancellationToken cancellationToken = default)
    {
        var tasksResult = await GetTasksAsync(new TaskFilterDto { StudentId = studentId }, access, referenceUtcNow, cancellationToken);
        if (!tasksResult.Succeeded || tasksResult.Data is null)
        {
            return ServiceResult<TaskSummaryStatsDto>.Failure(tasksResult.Message ?? "Could not load task statistics.", tasksResult.ErrorType);
        }

        var items = tasksResult.Data;
        var total = items.Count;
        var pending = items.Count(t => t.Status == StudentTaskStatus.Pending);
        var inProgress = items.Count(t => t.Status == StudentTaskStatus.InProgress);
        var completed = items.Count(t => t.Status == StudentTaskStatus.Completed);
        var overdue = items.Count(t => t.Status == StudentTaskStatus.Overdue);
        var dueSoon = items.Count(t => t.IsDueSoon);
        var highPriorityActive = items.Count(t => t.Priority == TaskPriority.High && t.Status != StudentTaskStatus.Completed);
        var completionRate = total > 0 ? Math.Round((decimal)completed * 100m / total, 1) : 0.0m;

        return ServiceResult<TaskSummaryStatsDto>.Success(new TaskSummaryStatsDto(
            TotalTasks: total,
            PendingCount: pending,
            InProgressCount: inProgress,
            CompletedCount: completed,
            OverdueCount: overdue,
            DueSoonCount: dueSoon,
            HighPriorityActiveCount: highPriorityActive,
            CompletionRatePercentage: completionRate));
    }

    public async Task<ServiceResult<StudentTaskDto>> CreateTaskAsync(
        SaveStudentTaskRequestDto request,
        AccessContext access,
        DateTime? referenceUtcNow = null,
        CancellationToken cancellationToken = default)
    {
        if (!access.IsAuthenticated)
        {
            return ServiceResult<StudentTaskDto>.Failure("Authentication is required.", ServiceErrorType.Unauthorized);
        }

        if (!access.IsAdmin)
        {
            if (!access.StudentId.HasValue)
            {
                return ServiceResult<StudentTaskDto>.Failure("Student profile is required to create tasks.", ServiceErrorType.Forbidden);
            }

            if (request.StudentId <= 0)
            {
                request.StudentId = access.StudentId.Value;
            }
            else if (!access.CanAccessStudent(request.StudentId))
            {
                return ServiceResult<StudentTaskDto>.Failure("Students may only create tasks for their own profile.", ServiceErrorType.Forbidden);
            }
        }

        var errors = TaskNotificationValidator.ValidateTask(request);
        if (errors.Count > 0)
        {
            return ServiceResult<StudentTaskDto>.Failure(errors, ServiceErrorType.Validation);
        }

        var student = await _studentRepo.GetByIdAsync(request.StudentId, cancellationToken);
        if (student is null)
        {
            return ServiceResult<StudentTaskDto>.Failure("Assigned student does not exist.", ServiceErrorType.NotFound);
        }

        if (request.CourseId.HasValue && request.CourseId.Value > 0)
        {
            var course = await _academicRepo.GetCourseByIdAsync(request.CourseId.Value, cancellationToken);
            if (course is null)
            {
                return ServiceResult<StudentTaskDto>.Failure("Selected course does not exist.", ServiceErrorType.NotFound);
            }
        }
        else
        {
            request.CourseId = null;
        }

        var now = referenceUtcNow ?? DateTime.UtcNow;
        var entity = new StudentTask
        {
            StudentId = request.StudentId,
            CourseId = request.CourseId,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            DueDate = request.DueDate,
            Priority = request.Priority,
            Status = request.Status,
            CreatedAt = now,
            UpdatedAt = now,
            CompletedAt = request.Status == StudentTaskStatus.Completed ? now : null
        };

        var saved = await _taskNotificationRepo.AddTaskAsync(entity, cancellationToken);
        var reloaded = await _taskNotificationRepo.GetTaskByIdAsync(saved.Id, cancellationToken) ?? saved;

        // If an Admin creates/assigns a task for a student, generate a TaskAssigned notification
        if (access.IsAdmin)
        {
            var coursePrefix = reloaded.Course is not null ? $"[{reloaded.Course.CourseCode}] " : string.Empty;
            await _taskNotificationRepo.AddNotificationAsync(new Notification
            {
                UserId = student.UserId,
                StudentId = student.Id,
                RelatedTaskId = reloaded.Id,
                RelatedCourseId = reloaded.CourseId,
                Title = $"New Academic Task Assigned: {coursePrefix}{reloaded.Title}",
                Message = $"A new {reloaded.Priority} priority task '{reloaded.Title}' has been assigned to you with deadline {reloaded.DueDate:yyyy-MM-dd HH:mm}.",
                Type = NotificationType.TaskAssigned,
                IsRead = false,
                CreatedAt = now
            }, cancellationToken);
        }

        // Also synchronize deadline / overdue notifications for this student
        await GenerateTaskDeadlineAlertIfNeededAsync(reloaded, student.UserId, now, cancellationToken);

        return ServiceResult<StudentTaskDto>.Success(MapTaskDto(reloaded, now), "Task created successfully.");
    }

    public async Task<ServiceResult<StudentTaskDto>> UpdateTaskAsync(
        int taskId,
        SaveStudentTaskRequestDto request,
        AccessContext access,
        DateTime? referenceUtcNow = null,
        CancellationToken cancellationToken = default)
    {
        if (!access.IsAuthenticated)
        {
            return ServiceResult<StudentTaskDto>.Failure("Authentication is required.", ServiceErrorType.Unauthorized);
        }

        var existing = await _taskNotificationRepo.GetTaskByIdAsync(taskId, cancellationToken);
        if (existing is null)
        {
            return ServiceResult<StudentTaskDto>.Failure("Task not found.", ServiceErrorType.NotFound);
        }

        if (!access.CanAccessStudent(existing.StudentId))
        {
            return ServiceResult<StudentTaskDto>.Failure("You do not have permission to edit this task.", ServiceErrorType.Forbidden);
        }

        if (!access.IsAdmin)
        {
            request.StudentId = existing.StudentId;
        }

        var errors = TaskNotificationValidator.ValidateTask(request);
        if (errors.Count > 0)
        {
            return ServiceResult<StudentTaskDto>.Failure(errors, ServiceErrorType.Validation);
        }

        if (request.CourseId.HasValue && request.CourseId.Value > 0)
        {
            var course = await _academicRepo.GetCourseByIdAsync(request.CourseId.Value, cancellationToken);
            if (course is null)
            {
                return ServiceResult<StudentTaskDto>.Failure("Selected course does not exist.", ServiceErrorType.NotFound);
            }
        }
        else
        {
            request.CourseId = null;
        }

        var now = referenceUtcNow ?? DateTime.UtcNow;
        existing.StudentId = request.StudentId;
        existing.CourseId = request.CourseId;
        existing.Title = request.Title.Trim();
        existing.Description = request.Description.Trim();
        existing.DueDate = request.DueDate;
        existing.Priority = request.Priority;
        existing.Status = request.Status;
        existing.UpdatedAt = now;
        existing.CompletedAt = request.Status == StudentTaskStatus.Completed
            ? (existing.CompletedAt ?? now)
            : null;

        await _taskNotificationRepo.UpdateTaskAsync(existing, cancellationToken);
        var reloaded = await _taskNotificationRepo.GetTaskByIdAsync(existing.Id, cancellationToken) ?? existing;

        await GenerateTaskDeadlineAlertIfNeededAsync(reloaded, reloaded.Student?.UserId, now, cancellationToken);

        return ServiceResult<StudentTaskDto>.Success(MapTaskDto(reloaded, now), "Task updated successfully.");
    }

    public async Task<ServiceResult<StudentTaskDto>> UpdateTaskStatusAsync(
        int taskId,
        StudentTaskStatus status,
        AccessContext access,
        DateTime? referenceUtcNow = null,
        CancellationToken cancellationToken = default)
    {
        if (!access.IsAuthenticated)
        {
            return ServiceResult<StudentTaskDto>.Failure("Authentication is required.", ServiceErrorType.Unauthorized);
        }

        var existing = await _taskNotificationRepo.GetTaskByIdAsync(taskId, cancellationToken);
        if (existing is null)
        {
            return ServiceResult<StudentTaskDto>.Failure("Task not found.", ServiceErrorType.NotFound);
        }

        if (!access.CanAccessStudent(existing.StudentId))
        {
            return ServiceResult<StudentTaskDto>.Failure("You do not have permission to update this task.", ServiceErrorType.Forbidden);
        }

        var now = referenceUtcNow ?? DateTime.UtcNow;
        existing.Status = status;
        existing.UpdatedAt = now;
        existing.CompletedAt = status == StudentTaskStatus.Completed ? now : null;

        await _taskNotificationRepo.UpdateTaskAsync(existing, cancellationToken);
        return ServiceResult<StudentTaskDto>.Success(MapTaskDto(existing, now), $"Task marked as {status}.");
    }

    public async Task<ServiceResult> DeleteTaskAsync(
        int taskId,
        AccessContext access,
        CancellationToken cancellationToken = default)
    {
        if (!access.IsAuthenticated)
        {
            return ServiceResult.Failure("Authentication is required.", ServiceErrorType.Unauthorized);
        }

        var existing = await _taskNotificationRepo.GetTaskByIdAsync(taskId, cancellationToken);
        if (existing is null)
        {
            return ServiceResult.Failure("Task not found.", ServiceErrorType.NotFound);
        }

        if (!access.CanAccessStudent(existing.StudentId))
        {
            return ServiceResult.Failure("You do not have permission to delete this task.", ServiceErrorType.Forbidden);
        }

        await _taskNotificationRepo.DeleteTaskAsync(existing, cancellationToken);
        return ServiceResult.Success("Task deleted successfully.");
    }

    public async Task<ServiceResult<AcademicCalendarMonthDto>> GetCalendarMonthAsync(
        int? studentId,
        int year,
        int month,
        AccessContext access,
        DateTime? referenceUtcNow = null,
        CancellationToken cancellationToken = default)
    {
        if (!access.IsAuthenticated)
        {
            return ServiceResult<AcademicCalendarMonthDto>.Failure("Authentication is required.", ServiceErrorType.Unauthorized);
        }

        var targetYear = Math.Clamp(year, 2024, 2032);
        var targetMonth = Math.Clamp(month, 1, 12);
        var now = referenceUtcNow ?? DateTime.UtcNow;

        int? targetStudentId = studentId;
        if (!access.IsAdmin)
        {
            if (targetStudentId.HasValue && !access.CanAccessStudent(targetStudentId.Value))
            {
                return ServiceResult<AcademicCalendarMonthDto>.Failure("Students may only view their own calendar.", ServiceErrorType.Forbidden);
            }

            targetStudentId = access.StudentId;
        }

        var tasksResult = await GetTasksAsync(new TaskFilterDto { StudentId = targetStudentId, SortBy = "deadline_asc" }, access, now, cancellationToken);
        var allTasks = tasksResult.Succeeded && tasksResult.Data is not null ? tasksResult.Data : [];

        var scheduleEntries = await _academicRepo.GetScheduleEntriesAsync(targetStudentId, null, cancellationToken);

        var events = new List<CalendarDayEventDto>();
        var daysInMonth = DateTime.DaysInMonth(targetYear, targetMonth);

        for (int day = 1; day <= daysInMonth; day++)
        {
            var date = new DateOnly(targetYear, targetMonth, day);
            var dayOfWeek = date.DayOfWeek;

            // Add recurring weekly classes for that day of week
            foreach (var slot in scheduleEntries.Where(s => s.DayOfWeek == dayOfWeek).OrderBy(s => s.StartTime))
            {
                events.Add(new CalendarDayEventDto(
                    Date: date,
                    EventCategory: "Class",
                    Title: $"{slot.Course?.CourseCode ?? "CRS"}: {slot.Course?.Title ?? "Class Session"}",
                    Subtitle: $"{slot.StartTime:HH:mm}–{slot.EndTime:HH:mm} • Room {slot.Classroom}",
                    CourseCode: slot.Course?.CourseCode,
                    BadgeText: slot.LessonType,
                    BadgeVariant: "info",
                    TaskId: null,
                    IsCompleted: false,
                    IsOverdue: false));
            }

            // Add task deadlines falling on this date
            foreach (var task in allTasks.Where(t => DateOnly.FromDateTime(t.DueDate) == date))
            {
                var variant = task.Status switch
                {
                    StudentTaskStatus.Completed => "success",
                    StudentTaskStatus.Overdue => "danger",
                    _ when task.Priority == TaskPriority.High => "warning",
                    _ => "primary"
                };

                events.Add(new CalendarDayEventDto(
                    Date: date,
                    EventCategory: "Deadline",
                    Title: task.Title,
                    Subtitle: $"Due {task.DueDate:HH:mm} • {task.Priority} Priority",
                    CourseCode: task.CourseCode,
                    BadgeText: task.Status.ToString(),
                    BadgeVariant: variant,
                    TaskId: task.Id,
                    IsCompleted: task.Status == StudentTaskStatus.Completed,
                    IsOverdue: task.IsOverdue));
            }
        }

        var upcomingDeadlines = allTasks
            .Where(t => t.Status != StudentTaskStatus.Completed && t.DueDate >= now)
            .OrderBy(t => t.DueDate)
            .Take(8)
            .ToList();

        var overdueTasks = allTasks
            .Where(t => t.IsOverdue)
            .OrderBy(t => t.DueDate)
            .ToList();

        var monthTitle = new DateTime(targetYear, targetMonth, 1).ToString("MMMM yyyy", CultureInfo.InvariantCulture);

        return ServiceResult<AcademicCalendarMonthDto>.Success(new AcademicCalendarMonthDto(
            Year: targetYear,
            Month: targetMonth,
            MonthTitle: monthTitle,
            Events: events,
            UpcomingDeadlines: upcomingDeadlines,
            OverdueTasks: overdueTasks));
    }

    public async Task<ServiceResult<IReadOnlyList<NotificationDto>>> GetNotificationsAsync(
        int? studentId,
        bool unreadOnly,
        AccessContext access,
        CancellationToken cancellationToken = default)
    {
        if (!access.IsAuthenticated)
        {
            return ServiceResult<IReadOnlyList<NotificationDto>>.Failure("Authentication is required.", ServiceErrorType.Unauthorized);
        }

        int? targetStudentId = studentId;
        int? targetUserId = null;

        if (!access.IsAdmin)
        {
            if (targetStudentId.HasValue && !access.CanAccessStudent(targetStudentId.Value))
            {
                return ServiceResult<IReadOnlyList<NotificationDto>>.Failure("Students may only view their own notifications.", ServiceErrorType.Forbidden);
            }

            targetStudentId = access.StudentId;
            targetUserId = access.UserId;
        }

        var notifications = await _taskNotificationRepo.GetNotificationsAsync(targetStudentId, targetUserId, unreadOnly, cancellationToken);
        var dtos = notifications.Select(MapNotificationDto).ToList();

        return ServiceResult<IReadOnlyList<NotificationDto>>.Success(dtos);
    }

    public async Task<ServiceResult<int>> GetUnreadCountAsync(
        int? studentId,
        AccessContext access,
        CancellationToken cancellationToken = default)
    {
        var result = await GetNotificationsAsync(studentId, unreadOnly: true, access, cancellationToken);
        if (!result.Succeeded || result.Data is null)
        {
            return ServiceResult<int>.Failure(result.Message ?? "Failed to get unread count.", result.ErrorType);
        }

        return ServiceResult<int>.Success(result.Data.Count);
    }

    public async Task<ServiceResult<int>> SynchronizeDeadlineNotificationsAsync(
        int? studentId,
        AccessContext access,
        DateTime? referenceUtcNow = null,
        CancellationToken cancellationToken = default)
    {
        if (!access.IsAuthenticated)
        {
            return ServiceResult<int>.Failure("Authentication is required.", ServiceErrorType.Unauthorized);
        }

        int? targetStudentId = studentId;
        if (!access.IsAdmin)
        {
            if (targetStudentId.HasValue && !access.CanAccessStudent(targetStudentId.Value))
            {
                return ServiceResult<int>.Failure("Students may only synchronize notifications for their own account.", ServiceErrorType.Forbidden);
            }

            targetStudentId = access.StudentId;
        }

        var now = referenceUtcNow ?? DateTime.UtcNow;
        var tasks = await _taskNotificationRepo.GetTasksAsync(targetStudentId, null, cancellationToken);
        int generatedCount = 0;

        foreach (var task in tasks)
        {
            var created = await GenerateTaskDeadlineAlertIfNeededAsync(task, task.Student?.UserId, now, cancellationToken);
            if (created)
            {
                generatedCount++;
            }
        }

        return ServiceResult<int>.Success(generatedCount, $"Synchronized deadline notifications ({generatedCount} new alerts generated).");
    }

    public async Task<ServiceResult<NotificationDto>> CreateNotificationAsync(
        CreateNotificationRequestDto request,
        AccessContext access,
        CancellationToken cancellationToken = default)
    {
        if (!access.IsAuthenticated)
        {
            return ServiceResult<NotificationDto>.Failure("Authentication is required.", ServiceErrorType.Unauthorized);
        }

        if (!access.IsAdmin)
        {
            return ServiceResult<NotificationDto>.Failure("Only administrators can manually dispatch system notifications.", ServiceErrorType.Forbidden);
        }

        var errors = TaskNotificationValidator.ValidateNotification(request);
        if (errors.Count > 0)
        {
            return ServiceResult<NotificationDto>.Failure(errors, ServiceErrorType.Validation);
        }

        int? resolvedUserId = request.UserId;
        if (request.StudentId.HasValue && request.StudentId.Value > 0)
        {
            var student = await _studentRepo.GetByIdAsync(request.StudentId.Value, cancellationToken);
            if (student is null)
            {
                return ServiceResult<NotificationDto>.Failure("Target student does not exist.", ServiceErrorType.NotFound);
            }

            resolvedUserId ??= student.UserId;
        }

        var entity = new Notification
        {
            StudentId = request.StudentId,
            UserId = resolvedUserId,
            RelatedTaskId = request.RelatedTaskId,
            RelatedCourseId = request.RelatedCourseId,
            Title = request.Title.Trim(),
            Message = request.Message.Trim(),
            Type = request.Type,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        var saved = await _taskNotificationRepo.AddNotificationAsync(entity, cancellationToken);
        var reloaded = await _taskNotificationRepo.GetNotificationByIdAsync(saved.Id, cancellationToken) ?? saved;
        return ServiceResult<NotificationDto>.Success(MapNotificationDto(reloaded), "Notification sent.");
    }

    public async Task<ServiceResult<NotificationDto>> MarkNotificationReadStateAsync(
        int notificationId,
        bool isRead,
        AccessContext access,
        CancellationToken cancellationToken = default)
    {
        if (!access.IsAuthenticated)
        {
            return ServiceResult<NotificationDto>.Failure("Authentication is required.", ServiceErrorType.Unauthorized);
        }

        var existing = await _taskNotificationRepo.GetNotificationByIdAsync(notificationId, cancellationToken);
        if (existing is null)
        {
            return ServiceResult<NotificationDto>.Failure("Notification not found.", ServiceErrorType.NotFound);
        }

        if (!CanAccessNotification(existing, access))
        {
            return ServiceResult<NotificationDto>.Failure("You do not have permission to modify this notification.", ServiceErrorType.Forbidden);
        }

        existing.IsRead = isRead;
        existing.ReadAt = isRead ? DateTime.UtcNow : null;

        await _taskNotificationRepo.UpdateNotificationAsync(existing, cancellationToken);
        return ServiceResult<NotificationDto>.Success(MapNotificationDto(existing));
    }

    public async Task<ServiceResult<int>> MarkAllNotificationsAsReadAsync(
        int? studentId,
        AccessContext access,
        CancellationToken cancellationToken = default)
    {
        if (!access.IsAuthenticated)
        {
            return ServiceResult<int>.Failure("Authentication is required.", ServiceErrorType.Unauthorized);
        }

        int? targetStudentId = studentId;
        int? targetUserId = null;

        if (!access.IsAdmin)
        {
            if (targetStudentId.HasValue && !access.CanAccessStudent(targetStudentId.Value))
            {
                return ServiceResult<int>.Failure("Students may only mark their own notifications as read.", ServiceErrorType.Forbidden);
            }

            targetStudentId = access.StudentId;
            targetUserId = access.UserId;
        }

        var count = await _taskNotificationRepo.MarkAllAsReadAsync(targetStudentId, targetUserId, DateTime.UtcNow, cancellationToken);
        return ServiceResult<int>.Success(count, $"Marked {count} notification(s) as read.");
    }

    public async Task<ServiceResult> DeleteNotificationAsync(
        int notificationId,
        AccessContext access,
        CancellationToken cancellationToken = default)
    {
        if (!access.IsAuthenticated)
        {
            return ServiceResult.Failure("Authentication is required.", ServiceErrorType.Unauthorized);
        }

        var existing = await _taskNotificationRepo.GetNotificationByIdAsync(notificationId, cancellationToken);
        if (existing is null)
        {
            return ServiceResult.Failure("Notification not found.", ServiceErrorType.NotFound);
        }

        if (!CanAccessNotification(existing, access))
        {
            return ServiceResult.Failure("You do not have permission to delete this notification.", ServiceErrorType.Forbidden);
        }

        await _taskNotificationRepo.DeleteNotificationAsync(existing, cancellationToken);
        return ServiceResult.Success("Notification deleted.");
    }

    private async Task<bool> GenerateTaskDeadlineAlertIfNeededAsync(
        StudentTask task,
        int? userId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        if (task.Status == StudentTaskStatus.Completed)
        {
            return false;
        }

        var effectiveStatus = task.GetEffectiveStatus(now);
        var coursePrefix = task.Course is not null ? $"[{task.Course.CourseCode}] " : string.Empty;

        if (effectiveStatus == StudentTaskStatus.Overdue)
        {
            if (task.Status != StudentTaskStatus.Overdue)
            {
                task.Status = StudentTaskStatus.Overdue;
                task.UpdatedAt = now;
                await _taskNotificationRepo.UpdateTaskAsync(task, cancellationToken);
            }

            var alreadyAlerted = await _taskNotificationRepo.NotificationExistsForTaskAsync(
                task.StudentId,
                task.Id,
                NotificationType.OverdueAlert,
                cancellationToken);

            if (!alreadyAlerted)
            {
                await _taskNotificationRepo.AddNotificationAsync(new Notification
                {
                    UserId = userId,
                    StudentId = task.StudentId,
                    RelatedTaskId = task.Id,
                    RelatedCourseId = task.CourseId,
                    Title = $"Overdue Task Alert: {coursePrefix}{task.Title}",
                    Message = $"Your task '{task.Title}' passed its deadline on {task.DueDate:yyyy-MM-dd HH:mm} and is now marked Overdue.",
                    Type = NotificationType.OverdueAlert,
                    IsRead = false,
                    CreatedAt = now
                }, cancellationToken);
                return true;
            }
        }
        else if (task.DueDate >= now && task.DueDate <= now.AddHours(72))
        {
            var alreadyReminded = await _taskNotificationRepo.NotificationExistsForTaskAsync(
                task.StudentId,
                task.Id,
                NotificationType.DeadlineReminder,
                cancellationToken);

            if (!alreadyReminded)
            {
                var hoursRemaining = Math.Max(1, (int)Math.Round((task.DueDate - now).TotalHours));
                await _taskNotificationRepo.AddNotificationAsync(new Notification
                {
                    UserId = userId,
                    StudentId = task.StudentId,
                    RelatedTaskId = task.Id,
                    RelatedCourseId = task.CourseId,
                    Title = $"Upcoming Deadline ({hoursRemaining}h left): {coursePrefix}{task.Title}",
                    Message = $"Reminder: '{task.Title}' ({task.Priority} priority) is due on {task.DueDate:yyyy-MM-dd HH:mm}.",
                    Type = NotificationType.DeadlineReminder,
                    IsRead = false,
                    CreatedAt = now
                }, cancellationToken);
                return true;
            }
        }

        return false;
    }

    private static bool CanAccessNotification(Notification notification, AccessContext access)
    {
        if (access.IsAdmin)
        {
            return true;
        }

        if (notification.StudentId.HasValue && access.CanAccessStudent(notification.StudentId.Value))
        {
            return true;
        }

        if (notification.UserId.HasValue && access.UserId == notification.UserId.Value)
        {
            return true;
        }

        return false;
    }

    private static StudentTaskDto MapTaskDto(StudentTask task, DateTime utcNow)
    {
        var effectiveStatus = task.GetEffectiveStatus(utcNow);
        var isOverdue = effectiveStatus == StudentTaskStatus.Overdue;
        var isDueSoon = !isOverdue
            && effectiveStatus != StudentTaskStatus.Completed
            && task.DueDate >= utcNow
            && task.DueDate <= utcNow.AddHours(72);

        return new StudentTaskDto(
            Id: task.Id,
            StudentId: task.StudentId,
            StudentName: task.Student?.FullName ?? $"Student #{task.StudentId}",
            StudentNumber: task.Student?.StudentNumber ?? $"STU-{task.StudentId:D4}",
            CourseId: task.CourseId,
            CourseCode: task.Course?.CourseCode,
            CourseTitle: task.Course?.Title,
            Title: task.Title,
            Description: task.Description,
            DueDate: task.DueDate,
            Priority: task.Priority,
            Status: effectiveStatus,
            IsOverdue: isOverdue,
            IsDueSoon: isDueSoon,
            CreatedAt: task.CreatedAt,
            UpdatedAt: task.UpdatedAt,
            CompletedAt: task.CompletedAt);
    }

    private static NotificationDto MapNotificationDto(Notification n) => new(
        Id: n.Id,
        UserId: n.UserId,
        StudentId: n.StudentId,
        StudentName: n.Student?.FullName,
        RelatedTaskId: n.RelatedTaskId,
        RelatedTaskTitle: n.RelatedTask?.Title,
        RelatedCourseId: n.RelatedCourseId,
        RelatedCourseCode: n.RelatedCourse?.CourseCode,
        Title: n.Title,
        Message: n.Message,
        Type: n.Type,
        IsRead: n.IsRead,
        CreatedAt: n.CreatedAt,
        ReadAt: n.ReadAt);
}
