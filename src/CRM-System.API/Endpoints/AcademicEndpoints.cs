using CRM_System.Core.Academic.DTOs;
using CRM_System.Core.Academic.Interfaces;
using CRM_System.Core.Academic.Models;
using CRM_System.Core.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace CRM_System.API.Endpoints;

public static class AcademicEndpoints
{
    public static IEndpointRouteBuilder MapAcademicEndpoints(this IEndpointRouteBuilder app)
    {
        // Teachers
        var teachersGroup = app.MapGroup("/api/teachers").WithTags("Teachers");

        teachersGroup.MapGet("/", async (HttpContext httpContext, IAcademicService academicService, CancellationToken ct) =>
        {
            var access = StudentEndpoints.ResolveAccessContext(httpContext.User);
            var result = await academicService.GetTeachersAsync(access, ct);
            return result.Succeeded ? Results.Ok(result.Data) : ToProblem(result);
        }).WithSummary("Lists all university teachers and their assigned courses.");

        teachersGroup.MapPost("/", async ([FromBody] SaveTeacherRequestDto request, HttpContext httpContext, IAcademicService academicService, CancellationToken ct) =>
        {
            var access = StudentEndpoints.ResolveAccessContext(httpContext.User);
            var result = await academicService.CreateTeacherAsync(request, access, ct);
            return result.Succeeded && result.Data is not null
                ? Results.Created($"/api/teachers/{result.Data.Id}", result.Data)
                : ToProblem(result);
        }).WithSummary("Creates a new teacher profile (Administrator only).");

        teachersGroup.MapPut("/{id:int}", async (int id, [FromBody] SaveTeacherRequestDto request, HttpContext httpContext, IAcademicService academicService, CancellationToken ct) =>
        {
            var access = StudentEndpoints.ResolveAccessContext(httpContext.User);
            var result = await academicService.UpdateTeacherAsync(id, request, access, ct);
            return result.Succeeded ? Results.Ok(result.Data) : ToProblem(result);
        }).WithSummary("Updates an existing teacher profile (Administrator only).");

        teachersGroup.MapDelete("/{id:int}", async (int id, HttpContext httpContext, IAcademicService academicService, CancellationToken ct) =>
        {
            var access = StudentEndpoints.ResolveAccessContext(httpContext.User);
            var result = await academicService.DeleteTeacherAsync(id, access, ct);
            return result.Succeeded ? Results.Ok(new { message = result.Message }) : ToProblem(result);
        }).WithSummary("Deletes a teacher profile (Administrator only).");

        // Courses
        var coursesGroup = app.MapGroup("/api/courses").WithTags("Courses");

        coursesGroup.MapGet("/", async ([FromQuery] string? searchTerm, [FromQuery] string? department, HttpContext httpContext, IAcademicService academicService, CancellationToken ct) =>
        {
            var access = StudentEndpoints.ResolveAccessContext(httpContext.User);
            var result = await academicService.GetCoursesAsync(searchTerm, department, access, ct);
            return result.Succeeded ? Results.Ok(result.Data) : ToProblem(result);
        }).WithSummary("Lists and searches university courses.");

        coursesGroup.MapGet("/{id:int}", async (int id, HttpContext httpContext, IAcademicService academicService, CancellationToken ct) =>
        {
            var access = StudentEndpoints.ResolveAccessContext(httpContext.User);
            var result = await academicService.GetCourseByIdAsync(id, access, ct);
            return result.Succeeded ? Results.Ok(result.Data) : ToProblem(result);
        }).WithSummary("Gets course details by ID.");

        coursesGroup.MapPost("/", async ([FromBody] SaveCourseRequestDto request, HttpContext httpContext, IAcademicService academicService, CancellationToken ct) =>
        {
            var access = StudentEndpoints.ResolveAccessContext(httpContext.User);
            var result = await academicService.CreateCourseAsync(request, access, ct);
            return result.Succeeded && result.Data is not null
                ? Results.Created($"/api/courses/{result.Data.Id}", result.Data)
                : ToProblem(result);
        }).WithSummary("Creates a new course (Administrator only).");

        coursesGroup.MapPut("/{id:int}", async (int id, [FromBody] SaveCourseRequestDto request, HttpContext httpContext, IAcademicService academicService, CancellationToken ct) =>
        {
            var access = StudentEndpoints.ResolveAccessContext(httpContext.User);
            var result = await academicService.UpdateCourseAsync(id, request, access, ct);
            return result.Succeeded ? Results.Ok(result.Data) : ToProblem(result);
        }).WithSummary("Updates a course (Administrator only).");

        coursesGroup.MapDelete("/{id:int}", async (int id, HttpContext httpContext, IAcademicService academicService, CancellationToken ct) =>
        {
            var access = StudentEndpoints.ResolveAccessContext(httpContext.User);
            var result = await academicService.DeleteCourseAsync(id, access, ct);
            return result.Succeeded ? Results.Ok(new { message = result.Message }) : ToProblem(result);
        }).WithSummary("Deletes a course (Administrator only).");

        // Enrollments
        var enrollmentsGroup = app.MapGroup("/api/enrollments").WithTags("Enrollments");

        enrollmentsGroup.MapGet("/", async ([FromQuery] int? studentId, [FromQuery] int? courseId, HttpContext httpContext, IAcademicService academicService, CancellationToken ct) =>
        {
            var access = StudentEndpoints.ResolveAccessContext(httpContext.User);
            var result = await academicService.GetEnrollmentsAsync(studentId, courseId, access, ct);
            return result.Succeeded ? Results.Ok(result.Data) : ToProblem(result);
        }).WithSummary("Gets student course enrollments (Student owner or Administrator).");

        enrollmentsGroup.MapPost("/", async ([FromBody] CreateEnrollmentRequestDto request, HttpContext httpContext, IAcademicService academicService, CancellationToken ct) =>
        {
            var access = StudentEndpoints.ResolveAccessContext(httpContext.User);
            var result = await academicService.EnrollStudentAsync(request, access, ct);
            return result.Succeeded && result.Data is not null
                ? Results.Created($"/api/enrollments/{result.Data.Id}", result.Data)
                : ToProblem(result);
        }).WithSummary("Enrolls a student in a course.");

        enrollmentsGroup.MapPatch("/{id:int}/status", async (int id, [FromBody] UpdateEnrollmentStatusRequest statusRequest, HttpContext httpContext, IAcademicService academicService, CancellationToken ct) =>
        {
            var access = StudentEndpoints.ResolveAccessContext(httpContext.User);
            var result = await academicService.UpdateEnrollmentStatusAsync(id, statusRequest.Status, access, ct);
            return result.Succeeded ? Results.Ok(result.Data) : ToProblem(result);
        }).WithSummary("Updates enrollment status.");

        enrollmentsGroup.MapDelete("/{id:int}", async (int id, HttpContext httpContext, IAcademicService academicService, CancellationToken ct) =>
        {
            var access = StudentEndpoints.ResolveAccessContext(httpContext.User);
            var result = await academicService.RemoveEnrollmentAsync(id, access, ct);
            return result.Succeeded ? Results.Ok(new { message = result.Message }) : ToProblem(result);
        }).WithSummary("Removes a student course enrollment.");

        // Grades & GPA
        var gradesGroup = app.MapGroup("/api/grades").WithTags("Grades & GPA");

        gradesGroup.MapGet("/", async ([FromQuery] int? studentId, [FromQuery] int? courseId, HttpContext httpContext, IAcademicService academicService, CancellationToken ct) =>
        {
            var access = StudentEndpoints.ResolveAccessContext(httpContext.User);
            var result = await academicService.GetGradesAsync(studentId, courseId, access, ct);
            return result.Succeeded ? Results.Ok(result.Data) : ToProblem(result);
        }).WithSummary("Lists grades (Student owner or Administrator).");

        gradesGroup.MapGet("/gpa/{studentId:int}", async (int studentId, HttpContext httpContext, IAcademicService academicService, CancellationToken ct) =>
        {
            var access = StudentEndpoints.ResolveAccessContext(httpContext.User);
            var result = await academicService.GetStudentGpaSummaryAsync(studentId, access, ct);
            return result.Succeeded ? Results.Ok(result.Data) : ToProblem(result);
        }).WithSummary("Calculates a student's GPA and per-course grade averages using the configured grading scale.");

        gradesGroup.MapPost("/", async ([FromBody] SaveGradeRequestDto request, HttpContext httpContext, IAcademicService academicService, CancellationToken ct) =>
        {
            var access = StudentEndpoints.ResolveAccessContext(httpContext.User);
            var result = await academicService.AddGradeAsync(request, access, ct);
            return result.Succeeded && result.Data is not null
                ? Results.Created($"/api/grades/{result.Data.Id}", result.Data)
                : ToProblem(result);
        }).WithSummary("Records a new grade for a student (Administrator only).");

        gradesGroup.MapPut("/{id:int}", async (int id, [FromBody] SaveGradeRequestDto request, HttpContext httpContext, IAcademicService academicService, CancellationToken ct) =>
        {
            var access = StudentEndpoints.ResolveAccessContext(httpContext.User);
            var result = await academicService.UpdateGradeAsync(id, request, access, ct);
            return result.Succeeded ? Results.Ok(result.Data) : ToProblem(result);
        }).WithSummary("Updates an existing grade (Administrator only).");

        gradesGroup.MapDelete("/{id:int}", async (int id, HttpContext httpContext, IAcademicService academicService, CancellationToken ct) =>
        {
            var access = StudentEndpoints.ResolveAccessContext(httpContext.User);
            var result = await academicService.DeleteGradeAsync(id, access, ct);
            return result.Succeeded ? Results.Ok(new { message = result.Message }) : ToProblem(result);
        }).WithSummary("Deletes a grade record (Administrator only).");

        // Schedule
        var scheduleGroup = app.MapGroup("/api/schedule").WithTags("Schedule");

        scheduleGroup.MapGet("/", async ([FromQuery] int? studentId, [FromQuery] int? courseId, HttpContext httpContext, IAcademicService academicService, CancellationToken ct) =>
        {
            var access = StudentEndpoints.ResolveAccessContext(httpContext.User);
            var result = await academicService.GetScheduleAsync(studentId, courseId, access, ct);
            return result.Succeeded ? Results.Ok(result.Data) : ToProblem(result);
        }).WithSummary("Returns weekly class schedule entries.");

        scheduleGroup.MapGet("/upcoming/{studentId:int}", async (int studentId, [FromQuery] int? maxCount, HttpContext httpContext, IAcademicService academicService, CancellationToken ct) =>
        {
            var access = StudentEndpoints.ResolveAccessContext(httpContext.User);
            var result = await academicService.GetUpcomingClassesAsync(studentId, DateTime.Now, maxCount ?? 5, access, ct);
            return result.Succeeded ? Results.Ok(result.Data) : ToProblem(result);
        }).WithSummary("Returns upcoming classes for an enrolled student.");

        scheduleGroup.MapPost("/", async ([FromBody] SaveScheduleEntryRequestDto request, HttpContext httpContext, IAcademicService academicService, CancellationToken ct) =>
        {
            var access = StudentEndpoints.ResolveAccessContext(httpContext.User);
            var result = await academicService.CreateScheduleEntryAsync(request, access, ct);
            return result.Succeeded && result.Data is not null
                ? Results.Created($"/api/schedule/{result.Data.Id}", result.Data)
                : ToProblem(result);
        }).WithSummary("Creates a weekly schedule entry (Administrator only).");

        scheduleGroup.MapPut("/{id:int}", async (int id, [FromBody] SaveScheduleEntryRequestDto request, HttpContext httpContext, IAcademicService academicService, CancellationToken ct) =>
        {
            var access = StudentEndpoints.ResolveAccessContext(httpContext.User);
            var result = await academicService.UpdateScheduleEntryAsync(id, request, access, ct);
            return result.Succeeded ? Results.Ok(result.Data) : ToProblem(result);
        }).WithSummary("Updates a weekly schedule entry (Administrator only).");

        scheduleGroup.MapDelete("/{id:int}", async (int id, HttpContext httpContext, IAcademicService academicService, CancellationToken ct) =>
        {
            var access = StudentEndpoints.ResolveAccessContext(httpContext.User);
            var result = await academicService.DeleteScheduleEntryAsync(id, access, ct);
            return result.Succeeded ? Results.Ok(new { message = result.Message }) : ToProblem(result);
        }).WithSummary("Deletes a weekly schedule entry (Administrator only).");

        // Attendance
        var attendanceGroup = app.MapGroup("/api/attendance").WithTags("Attendance");

        attendanceGroup.MapGet("/", async ([FromQuery] int? studentId, [FromQuery] int? courseId, HttpContext httpContext, IAcademicService academicService, CancellationToken ct) =>
        {
            var access = StudentEndpoints.ResolveAccessContext(httpContext.User);
            var result = await academicService.GetAttendanceRecordsAsync(studentId, courseId, access, ct);
            return result.Succeeded ? Results.Ok(result.Data) : ToProblem(result);
        }).WithSummary("Lists attendance records (Student owner or Administrator).");

        attendanceGroup.MapGet("/stats/{studentId:int}", async (int studentId, HttpContext httpContext, IAcademicService academicService, CancellationToken ct) =>
        {
            var access = StudentEndpoints.ResolveAccessContext(httpContext.User);
            var result = await academicService.GetStudentAttendanceStatsAsync(studentId, access, ct);
            return result.Succeeded ? Results.Ok(result.Data) : ToProblem(result);
        }).WithSummary("Calculates overall and per-course attendance statistics for a student.");

        attendanceGroup.MapPost("/", async ([FromBody] SaveAttendanceRequestDto request, HttpContext httpContext, IAcademicService academicService, CancellationToken ct) =>
        {
            var access = StudentEndpoints.ResolveAccessContext(httpContext.User);
            var result = await academicService.RecordAttendanceAsync(request, access, ct);
            return result.Succeeded && result.Data is not null
                ? Results.Created($"/api/attendance/{result.Data.Id}", result.Data)
                : ToProblem(result);
        }).WithSummary("Records student attendance (Administrator only).");

        attendanceGroup.MapPut("/{id:int}", async (int id, [FromBody] SaveAttendanceRequestDto request, HttpContext httpContext, IAcademicService academicService, CancellationToken ct) =>
        {
            var access = StudentEndpoints.ResolveAccessContext(httpContext.User);
            var result = await academicService.UpdateAttendanceAsync(id, request, access, ct);
            return result.Succeeded ? Results.Ok(result.Data) : ToProblem(result);
        }).WithSummary("Updates an attendance record (Administrator only).");

        attendanceGroup.MapDelete("/{id:int}", async (int id, HttpContext httpContext, IAcademicService academicService, CancellationToken ct) =>
        {
            var access = StudentEndpoints.ResolveAccessContext(httpContext.User);
            var result = await academicService.DeleteAttendanceAsync(id, access, ct);
            return result.Succeeded ? Results.Ok(new { message = result.Message }) : ToProblem(result);
        }).WithSummary("Deletes an attendance record (Administrator only).");

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

        return Results.Json(
            new
            {
                title = result.Message ?? "Request failed.",
                status = statusCode,
                errors = result.Errors
            },
            statusCode: statusCode);
    }

    public sealed record UpdateEnrollmentStatusRequest(EnrollmentStatus Status);
}
