using System.Security.Claims;
using CRM_System.Core.Common;
using CRM_System.Core.Students.DTOs;
using CRM_System.Core.Students.Interfaces;
using CRM_System.Core.Students.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace CRM_System.API.Endpoints;

public static class StudentEndpoints
{
    public static IEndpointRouteBuilder MapAuthAndStudentEndpoints(this IEndpointRouteBuilder app)
    {
        var authGroup = app.MapGroup("/api/auth")
            .WithTags("Authentication");

        authGroup.MapPost("/register", async (
            [FromBody] RegisterStudentRequestDto request,
            IAuthService authService,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var result = await authService.RegisterStudentAsync(request, ct);
            if (!result.Succeeded || result.Data is null)
            {
                return ToProblemResult(result);
            }

            await SignInCookieAsync(httpContext, result.Data, rememberMe: true);
            return Results.Created($"/api/students/{result.Data.StudentId}", result.Data);
        })
        .WithName("RegisterStudent")
        .WithSummary("Registers a new student account and creates their student profile.");

        authGroup.MapPost("/login", async (
            [FromBody] LoginRequestDto request,
            IAuthService authService,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var result = await authService.LoginAsync(request, ct);
            if (!result.Succeeded || result.Data is null)
            {
                return ToProblemResult(result);
            }

            await SignInCookieAsync(httpContext, result.Data, request.RememberMe);
            return Results.Ok(result.Data);
        })
        .WithName("LoginUser")
        .WithSummary("Authenticates a student or administrator and establishes a session.");

        authGroup.MapPost("/logout", async (HttpContext httpContext) =>
        {
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Ok(new { message = "Logged out successfully." });
        })
        .WithName("LogoutUser")
        .WithSummary("Signs out the current user session.");

        authGroup.MapGet("/me", async (
            HttpContext httpContext,
            IAuthService authService,
            CancellationToken ct) =>
        {
            var access = ResolveAccessContext(httpContext.User);
            if (!access.IsAuthenticated || !access.UserId.HasValue)
            {
                return Results.Unauthorized();
            }

            var user = await authService.GetUserByIdAsync(access.UserId.Value, ct);
            return user is not null ? Results.Ok(user) : Results.Unauthorized();
        })
        .WithName("GetCurrentUser")
        .WithSummary("Returns the currently authenticated user's profile and role.");

        authGroup.MapPost("/change-password", async (
            [FromBody] ChangePasswordRequestDto request,
            HttpContext httpContext,
            IAuthService authService,
            CancellationToken ct) =>
        {
            var access = ResolveAccessContext(httpContext.User);
            if (!access.IsAuthenticated || !access.UserId.HasValue)
            {
                return Results.Unauthorized();
            }

            var result = await authService.ChangePasswordAsync(access.UserId.Value, request, ct);
            return result.Succeeded
                ? Results.Ok(new { message = result.Message })
                : ToProblemResult(result);
        })
        .WithName("ChangePassword")
        .WithSummary("Updates the password of the currently authenticated user.");

        var studentsGroup = app.MapGroup("/api/students")
            .WithTags("Students");

        studentsGroup.MapGet("/", async (
            [FromQuery] string? searchTerm,
            [FromQuery] string? faculty,
            [FromQuery] int? studyYear,
            [FromQuery] StudentStatus? status,
            [FromQuery] string? sortBy,
            [FromQuery] bool? sortDescending,
            HttpContext httpContext,
            IStudentService studentService,
            CancellationToken ct) =>
        {
            var access = ResolveAccessContext(httpContext.User);
            var filter = new StudentFilterDto
            {
                SearchTerm = searchTerm,
                Faculty = faculty,
                StudyYear = studyYear,
                Status = status,
                SortBy = string.IsNullOrWhiteSpace(sortBy) ? "FullName" : sortBy,
                SortDescending = sortDescending ?? false
            };

            var result = await studentService.GetStudentsAsync(filter, access, ct);
            return result.Succeeded ? Results.Ok(result.Data) : ToProblemResult(result);
        })
        .WithName("GetStudents")
        .WithSummary("Lists and searches student records (Administrator only).");

        studentsGroup.MapGet("/stats", async (
            HttpContext httpContext,
            IStudentService studentService,
            CancellationToken ct) =>
        {
            var access = ResolveAccessContext(httpContext.User);
            var result = await studentService.GetDirectoryStatsAsync(access, ct);
            return result.Succeeded ? Results.Ok(result.Data) : ToProblemResult(result);
        })
        .WithName("GetStudentStats")
        .WithSummary("Returns summary counts for the student directory.");

        studentsGroup.MapGet("/faculties", async (
            IStudentService studentService,
            CancellationToken ct) =>
        {
            var faculties = await studentService.GetDistinctFacultiesAsync(ct);
            return Results.Ok(faculties);
        })
        .WithName("GetDistinctFaculties")
        .WithSummary("Returns the list of distinct university faculties.");

        studentsGroup.MapGet("/{id:int}", async (
            int id,
            HttpContext httpContext,
            IStudentService studentService,
            CancellationToken ct) =>
        {
            var access = ResolveAccessContext(httpContext.User);
            var result = await studentService.GetStudentByIdAsync(id, access, ct);
            return result.Succeeded ? Results.Ok(result.Data) : ToProblemResult(result);
        })
        .WithName("GetStudentById")
        .WithSummary("Gets a student profile by ID (accessible by Admin or the student owner).");

        studentsGroup.MapPost("/", async (
            [FromBody] CreateStudentRequestDto request,
            HttpContext httpContext,
            IStudentService studentService,
            CancellationToken ct) =>
        {
            var access = ResolveAccessContext(httpContext.User);
            var result = await studentService.CreateStudentAsync(request, access, ct);
            return result.Succeeded && result.Data is not null
                ? Results.Created($"/api/students/{result.Data.Id}", result.Data)
                : ToProblemResult(result);
        })
        .WithName("CreateStudent")
        .WithSummary("Creates a new student record (Administrator only).");

        studentsGroup.MapPut("/{id:int}", async (
            int id,
            [FromBody] UpdateStudentRequestDto request,
            HttpContext httpContext,
            IStudentService studentService,
            CancellationToken ct) =>
        {
            var access = ResolveAccessContext(httpContext.User);
            var result = await studentService.UpdateStudentAsync(id, request, access, ct);
            return result.Succeeded ? Results.Ok(result.Data) : ToProblemResult(result);
        })
        .WithName("UpdateStudent")
        .WithSummary("Updates a student record (Administrator only).");

        studentsGroup.MapPut("/{id:int}/profile", async (
            int id,
            [FromBody] UpdatePersonalProfileRequestDto request,
            HttpContext httpContext,
            IStudentService studentService,
            CancellationToken ct) =>
        {
            var access = ResolveAccessContext(httpContext.User);
            var result = await studentService.UpdatePersonalProfileAsync(id, request, access, ct);
            return result.Succeeded ? Results.Ok(result.Data) : ToProblemResult(result);
        })
        .WithName("UpdatePersonalProfile")
        .WithSummary("Updates permitted personal profile information (Student owner or Administrator).");

        studentsGroup.MapPatch("/{id:int}/status", async (
            int id,
            [FromBody] UpdateStudentStatusRequest statusRequest,
            HttpContext httpContext,
            IStudentService studentService,
            CancellationToken ct) =>
        {
            var access = ResolveAccessContext(httpContext.User);
            var result = await studentService.SetStudentStatusAsync(id, statusRequest.Status, access, ct);
            return result.Succeeded ? Results.Ok(result.Data) : ToProblemResult(result);
        })
        .WithName("SetStudentStatus")
        .WithSummary("Activates, deactivates, or updates a student's status (Administrator only).");

        studentsGroup.MapDelete("/{id:int}", async (
            int id,
            HttpContext httpContext,
            IStudentService studentService,
            CancellationToken ct) =>
        {
            var access = ResolveAccessContext(httpContext.User);
            var result = await studentService.DeleteStudentAsync(id, access, ct);
            return result.Succeeded
                ? Results.Ok(new { message = result.Message })
                : ToProblemResult(result);
        })
        .WithName("DeleteStudent")
        .WithSummary("Deletes a student record (Administrator only).");

        return app;
    }

    public static AccessContext ResolveAccessContext(ClaimsPrincipal principal)
    {
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return AccessContext.Anonymous;
        }

        var userIdStr = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var studentIdStr = principal.FindFirstValue("StudentId");
        var role = principal.FindFirstValue(ClaimTypes.Role) ?? UserRoles.Student;

        int? userId = int.TryParse(userIdStr, out var parsedUserId) ? parsedUserId : null;
        int? studentId = int.TryParse(studentIdStr, out var parsedStudentId) ? parsedStudentId : null;

        return new AccessContext(userId, studentId, role, true);
    }

    private static async Task SignInCookieAsync(
        HttpContext httpContext,
        AuthenticatedUserDto user,
        bool rememberMe)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        if (user.StudentId.HasValue)
        {
            claims.Add(new Claim("StudentId", user.StudentId.Value.ToString()));
        }

        if (!string.IsNullOrWhiteSpace(user.StudentNumber))
        {
            claims.Add(new Claim("StudentNumber", user.StudentNumber));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(rememberMe ? 7 : 1)
            });
    }

    private static IResult ToProblemResult(ServiceResult result)
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

    public sealed record UpdateStudentStatusRequest(StudentStatus Status);
}
