using CRM_System.Core.Common;
using CRM_System.Core.Dashboard.DTOs;
using CRM_System.Core.Dashboard.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CRM_System.API.Endpoints;

public static class DashboardAnalyticsEndpoints
{
    public static IEndpointRouteBuilder MapDashboardAnalyticsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/dashboard").WithTags("Dashboard & Analytics");

        group.MapGet("/overview", async (
            int? studentId,
            int? courseId,
            string? semester,
            DateOnly? dateFrom,
            DateOnly? dateTo,
            HttpContext httpContext,
            IDashboardAnalyticsService analyticsService,
            CancellationToken ct) =>
        {
            var access = StudentEndpoints.ResolveAccessContext(httpContext.User);
            var filter = new DashboardFilterDto
            {
                StudentId = studentId,
                CourseId = courseId,
                Semester = semester,
                DateFrom = dateFrom,
                DateTo = dateTo
            };

            var result = await analyticsService.GetDashboardOverviewAsync(filter, access, null, ct);
            return result.Succeeded ? Results.Ok(result.Data) : ToProblem(result);
        }).WithSummary("Returns the unified dashboard overview including KPIs, academic analytics, attendance analytics, task productivity, upcoming deadlines, notifications, and recent activity.");

        group.MapGet("/analytics/academic", async (
            int? studentId,
            int? courseId,
            string? semester,
            DateOnly? dateFrom,
            DateOnly? dateTo,
            HttpContext httpContext,
            IDashboardAnalyticsService analyticsService,
            CancellationToken ct) =>
        {
            var access = StudentEndpoints.ResolveAccessContext(httpContext.User);
            var filter = new DashboardFilterDto
            {
                StudentId = studentId,
                CourseId = courseId,
                Semester = semester,
                DateFrom = dateFrom,
                DateTo = dateTo
            };

            var result = await analyticsService.GetAcademicAnalyticsAsync(filter, access, ct);
            return result.Succeeded ? Results.Ok(result.Data) : ToProblem(result);
        }).WithSummary("Returns academic analytics including GPA statistics, GPA distribution, grade distribution, course performance, student rankings, and highest/lowest performers.");

        group.MapGet("/analytics/attendance", async (
            int? studentId,
            int? courseId,
            string? semester,
            DateOnly? dateFrom,
            DateOnly? dateTo,
            HttpContext httpContext,
            IDashboardAnalyticsService analyticsService,
            CancellationToken ct) =>
        {
            var access = StudentEndpoints.ResolveAccessContext(httpContext.User);
            var filter = new DashboardFilterDto
            {
                StudentId = studentId,
                CourseId = courseId,
                Semester = semester,
                DateFrom = dateFrom,
                DateTo = dateTo
            };

            var result = await analyticsService.GetAttendanceAnalyticsAsync(filter, access, ct);
            return result.Succeeded ? Results.Ok(result.Data) : ToProblem(result);
        }).WithSummary("Returns attendance analytics including overall rates, present/late/absent distribution, course and student attendance statistics, and chronological trends.");

        group.MapGet("/analytics/tasks", async (
            int? studentId,
            int? courseId,
            string? semester,
            DateOnly? dateFrom,
            DateOnly? dateTo,
            HttpContext httpContext,
            IDashboardAnalyticsService analyticsService,
            CancellationToken ct) =>
        {
            var access = StudentEndpoints.ResolveAccessContext(httpContext.User);
            var filter = new DashboardFilterDto
            {
                StudentId = studentId,
                CourseId = courseId,
                Semester = semester,
                DateFrom = dateFrom,
                DateTo = dateTo
            };

            var result = await analyticsService.GetTaskProductivityAnalyticsAsync(filter, access, null, ct);
            return result.Succeeded ? Results.Ok(result.Data) : ToProblem(result);
        }).WithSummary("Returns tasks and productivity analytics including completion rates, priority and status distributions, course task breakdowns, and upcoming/overdue tasks.");

        return app;
    }

    private static IResult ToProblem(ServiceResult result)
    {
        var statusCode = result.ErrorType switch
        {
            ServiceErrorType.Validation => StatusCodes.Status400BadRequest,
            ServiceErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ServiceErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ServiceErrorType.NotFound => StatusCodes.Status404NotFound,
            ServiceErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        return Results.Json(new
        {
            title = result.Message ?? "Request failed.",
            status = statusCode,
            errors = result.Errors
        }, statusCode: statusCode);
    }
}
