using CRM_System.API.Endpoints;
using CRM_System.Infrastructure;
using CRM_System.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddCrmInfrastructure(builder.Configuration);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "CRM.StudentAuth";
        options.Cookie.HttpOnly = true;
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

await app.Services.InitializeCrmDatabaseAsync();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/health", () => Results.Ok(new
{
    status = "Healthy",
    application = "CRM for Students API",
    stage = "Stage 4 - Dashboard and Analytics",
    timestampUtc = DateTime.UtcNow
}))
.WithName("GetApiHealth")
.WithTags("System");

app.MapAuthAndStudentEndpoints();
app.MapAcademicEndpoints();
app.MapTaskNotificationEndpoints();
app.MapDashboardAnalyticsEndpoints();

app.Run();

public partial class Program;
