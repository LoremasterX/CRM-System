using CRM_System.Core.Academic.Interfaces;
using CRM_System.Core.Academic.Models;
using CRM_System.Core.Academic.Services;
using CRM_System.Core.Students.Interfaces;
using CRM_System.Core.Students.Services;
using CRM_System.Core.Tasks.Interfaces;
using CRM_System.Core.Tasks.Services;
using CRM_System.Infrastructure.Academic;
using CRM_System.Infrastructure.Data;
using CRM_System.Infrastructure.Students;
using CRM_System.Infrastructure.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CRM_System.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCrmInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            var dbPath = Path.Combine(AppContext.BaseDirectory, "crm_students.db");
            connectionString = $"Data Source={dbPath}";
        }

        services.AddDbContext<CrmDbContext>(options =>
            options.UseSqlite(connectionString));

        // Configurable Grading Scale (Stage 2)
        var scaleOptions = new GradingScaleOptions();
        var scaleSection = configuration.GetSection(GradingScaleOptions.SectionName);
        if (scaleSection.Exists())
        {
            scaleSection.Bind(scaleOptions);
            if (scaleOptions.Bands.Count == 0)
            {
                scaleOptions.Bands = GradingScaleOptions.GetDefaultBands();
            }
        }
        services.AddSingleton(scaleOptions);
        services.AddSingleton<IGpaCalculator>(sp => new ConfigurableGpaCalculator(sp.GetRequiredService<GradingScaleOptions>()));

        // Stage 1: Student Management
        services.AddSingleton<IPasswordHasher, PasswordHasherAdapter>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IStudentService, StudentService>();

        // Stage 2: Academic Management
        services.AddScoped<IAcademicRepository, AcademicRepository>();
        services.AddScoped<IAcademicService, AcademicService>();

        // Stage 3: Tasks & Notifications
        services.AddScoped<ITaskNotificationRepository, TaskNotificationRepository>();
        services.AddScoped<ITaskNotificationService, TaskNotificationService>();

        return services;
    }
}
