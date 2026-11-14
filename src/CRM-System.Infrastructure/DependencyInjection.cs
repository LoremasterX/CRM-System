using CRM_System.Core.Students.Interfaces;
using CRM_System.Core.Students.Services;
using CRM_System.Infrastructure.Data;
using CRM_System.Infrastructure.Students;
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

        services.AddSingleton<IPasswordHasher, PasswordHasherAdapter>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IStudentService, StudentService>();

        return services;
    }
}
