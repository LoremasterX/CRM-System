using CRM_System.Core.Common;
using CRM_System.Core.Tasks.DTOs;
using CRM_System.Core.Tasks.Interfaces;
using CRM_System.Core.Tasks.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace CRM_System.API.Endpoints;

public static class TaskNotificationEndpoints
{
    public static IEndpointRouteBuilder MapTaskNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        // Tasks & Academic Calendar
        var tasksGroup = app.MapGroup("/api/tasks").WithTags("Tasks & Calendar");

        tasksGroup.MapGet("/", async (
            int? studentId,
            int? courseId,
            StudentTaskStatus? status,
            TaskPriority? priority,
            string? searchTerm,
            string? sortBy,
            HttpContext httpContext,
            ITaskNotificationService taskService,
            CancellationToken ct) =>
        {
            var access = StudentEndpoints.ResolveAccessContext(httpContext.User);
            var filter = new TaskFilterDto
            {
                StudentId = studentId,
                CourseId = courseId,
                Status = status,
                Priority = priority,
                SearchTerm = searchTerm,
                SortBy = string.IsNullOrWhiteSpace(sortBy) ? "deadline_asc" : sortBy
            };

            var result = await taskService.GetTasksAsync(filter, access, null, ct);
            return result.Succeeded ? Results.Ok(result.Data) : ToProblem(result);
        }).WithSummary("Lists student tasks with filtering by status, priority, course, search term, and deadline sorting.");

        tasksGroup.MapGet("/stats", async (
            int? studentId,
            HttpContext httpContext,
            ITaskNotificationService taskService,
            CancellationToken ct) =>
        {
            var access = StudentEndpoints.ResolveAccessContext(httpContext.User);
            var result = await taskService.GetTaskSummaryStatsAsync(studentId, access, null, ct);
            return result.Succeeded ? Results.Ok(result.Data) : ToProblem(result);
        }).WithSummary("Returns task completion and overdue statistics.");

        tasksGroup.MapGet("/{id:int}", async (
            int id,
            HttpContext httpContext,
            ITaskNotificationService taskService,
            CancellationToken ct) =>
        {
            var access = StudentEndpoints.ResolveAccessContext(httpContext.User);
            var result = await taskService.GetTaskByIdAsync(id, access, null, ct);
            return result.Succeeded ? Results.Ok(result.Data) : ToProblem(result);
        }).WithSummary("Gets a specific student task by ID.");

        tasksGroup.MapPost("/", async (
            [FromBody] SaveStudentTaskRequestDto request,
            HttpContext httpContext,
            ITaskNotificationService taskService,
            CancellationToken ct) =>
        {
            var access = StudentEndpoints.ResolveAccessContext(httpContext.User);
            var result = await taskService.CreateTaskAsync(request, access, null, ct);
            return result.Succeeded && result.Data is not null
                ? Results.Created($"/api/tasks/{result.Data.Id}", result.Data)
                : ToProblem(result);
        }).WithSummary("Creates a new academic or personal task.");

        tasksGroup.MapPut("/{id:int}", async (
            int id,
            [FromBody] SaveStudentTaskRequestDto request,
            HttpContext httpContext,
            ITaskNotificationService taskService,
            CancellationToken ct) =>
        {
            var access = StudentEndpoints.ResolveAccessContext(httpContext.User);
            var result = await taskService.UpdateTaskAsync(id, request, access, null, ct);
            return result.Succeeded ? Results.Ok(result.Data) : ToProblem(result);
        }).WithSummary("Updates an existing task.");

        tasksGroup.MapPatch("/{id:int}/status", async (
            int id,
            StudentTaskStatus status,
            HttpContext httpContext,
            ITaskNotificationService taskService,
            CancellationToken ct) =>
        {
            var access = StudentEndpoints.ResolveAccessContext(httpContext.User);
            var result = await taskService.UpdateTaskStatusAsync(id, status, access, null, ct);
            return result.Succeeded ? Results.Ok(result.Data) : ToProblem(result);
        }).WithSummary("Updates the status of a task (Pending, InProgress, Completed, Overdue).");

        tasksGroup.MapDelete("/{id:int}", async (
            int id,
            HttpContext httpContext,
            ITaskNotificationService taskService,
            CancellationToken ct) =>
        {
            var access = StudentEndpoints.ResolveAccessContext(httpContext.User);
            var result = await taskService.DeleteTaskAsync(id, access, ct);
            return result.Succeeded ? Results.Ok(new { message = result.Message }) : ToProblem(result);
        }).WithSummary("Deletes a task.");

        tasksGroup.MapGet("/calendar", async (
            int? studentId,
            int? year,
            int? month,
            HttpContext httpContext,
            ITaskNotificationService taskService,
            CancellationToken ct) =>
        {
            var access = StudentEndpoints.ResolveAccessContext(httpContext.User);
            var now = DateTime.UtcNow;
            var result = await taskService.GetCalendarMonthAsync(
                studentId,
                year ?? now.Year,
                month ?? now.Month,
                access,
                now,
                ct);
            return result.Succeeded ? Results.Ok(result.Data) : ToProblem(result);
        }).WithSummary("Returns monthly academic calendar combining weekly class schedules and task deadlines.");

        // Notifications
        var notificationsGroup = app.MapGroup("/api/notifications").WithTags("Notifications");

        notificationsGroup.MapGet("/", async (
            int? studentId,
            bool? unreadOnly,
            HttpContext httpContext,
            ITaskNotificationService taskService,
            CancellationToken ct) =>
        {
            var access = StudentEndpoints.ResolveAccessContext(httpContext.User);
            var result = await taskService.GetNotificationsAsync(studentId, unreadOnly ?? false, access, ct);
            return result.Succeeded ? Results.Ok(result.Data) : ToProblem(result);
        }).WithSummary("Lists persisted notifications for the authenticated student or administrator.");

        notificationsGroup.MapGet("/unread-count", async (
            int? studentId,
            HttpContext httpContext,
            ITaskNotificationService taskService,
            CancellationToken ct) =>
        {
            var access = StudentEndpoints.ResolveAccessContext(httpContext.User);
            var result = await taskService.GetUnreadCountAsync(studentId, access, ct);
            return result.Succeeded ? Results.Ok(new { unreadCount = result.Data }) : ToProblem(result);
        }).WithSummary("Gets the unread notification count.");

        notificationsGroup.MapPost("/sync-deadlines", async (
            int? studentId,
            HttpContext httpContext,
            ITaskNotificationService taskService,
            CancellationToken ct) =>
        {
            var access = StudentEndpoints.ResolveAccessContext(httpContext.User);
            var result = await taskService.SynchronizeDeadlineNotificationsAsync(studentId, access, null, ct);
            return result.Succeeded ? Results.Ok(new { generatedAlerts = result.Data, message = result.Message }) : ToProblem(result);
        }).WithSummary("Scans active tasks and generates upcoming deadline or overdue alerts idempotently.");

        notificationsGroup.MapPost("/", async (
            [FromBody] CreateNotificationRequestDto request,
            HttpContext httpContext,
            ITaskNotificationService taskService,
            CancellationToken ct) =>
        {
            var access = StudentEndpoints.ResolveAccessContext(httpContext.User);
            var result = await taskService.CreateNotificationAsync(request, access, ct);
            return result.Succeeded && result.Data is not null
                ? Results.Created($"/api/notifications/{result.Data.Id}", result.Data)
                : ToProblem(result);
        }).WithSummary("Dispatches a custom notification (Administrator only).");

        notificationsGroup.MapPatch("/{id:int}/read", async (
            int id,
            bool? isRead,
            HttpContext httpContext,
            ITaskNotificationService taskService,
            CancellationToken ct) =>
        {
            var access = StudentEndpoints.ResolveAccessContext(httpContext.User);
            var result = await taskService.MarkNotificationReadStateAsync(id, isRead ?? true, access, ct);
            return result.Succeeded ? Results.Ok(result.Data) : ToProblem(result);
        }).WithSummary("Marks a notification as read or unread.");

        notificationsGroup.MapPost("/mark-all-read", async (
            int? studentId,
            HttpContext httpContext,
            ITaskNotificationService taskService,
            CancellationToken ct) =>
        {
            var access = StudentEndpoints.ResolveAccessContext(httpContext.User);
            var result = await taskService.MarkAllNotificationsAsReadAsync(studentId, access, ct);
            return result.Succeeded ? Results.Ok(new { markedCount = result.Data, message = result.Message }) : ToProblem(result);
        }).WithSummary("Marks all unread notifications as read.");

        notificationsGroup.MapDelete("/{id:int}", async (
            int id,
            HttpContext httpContext,
            ITaskNotificationService taskService,
            CancellationToken ct) =>
        {
            var access = StudentEndpoints.ResolveAccessContext(httpContext.User);
            var result = await taskService.DeleteNotificationAsync(id, access, ct);
            return result.Succeeded ? Results.Ok(new { message = result.Message }) : ToProblem(result);
        }).WithSummary("Deletes a notification.");

        return app;
    }

    private static IResult ToProblem(ServiceResult result)
    {
        var statusCode = result.ErrorType switch
        {
            ServiceErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ServiceErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ServiceErrorType.NotFound => StatusCodes.Status404NotFound,
            ServiceErrorType.Conflict => StatusCodes.Status409Conflict,
            ServiceErrorType.Validation => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        return Results.Problem(
            title: result.ErrorType.ToString(),
            detail: result.Message,
            statusCode: statusCode,
            extensions: new Dictionary<string, object?> { ["errors"] = result.Errors });
    }
}
