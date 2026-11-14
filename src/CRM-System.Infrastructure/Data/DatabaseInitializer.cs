using CRM_System.Core.Students.Interfaces;
using CRM_System.Core.Students.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CRM_System.Infrastructure.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeCrmDatabaseAsync(
        this IServiceProvider services,
        bool seedDemoData = true,
        CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var logger = scope.ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger("DatabaseInitializer");
        var dbContext = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        if (dbContext.Database.IsRelational())
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        }
        else
        {
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        }

        if (seedDemoData)
        {
            await SeedInitialUsersAndStudentsAsync(dbContext, passwordHasher, logger, cancellationToken);
        }
    }

    public static async Task SeedInitialUsersAndStudentsAsync(
        CrmDbContext dbContext,
        IPasswordHasher passwordHasher,
        ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        if (await dbContext.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = DateTime.UtcNow;

        var adminUser = new ApplicationUser
        {
            Email = "admin@crm.edu",
            NormalizedEmail = "ADMIN@CRM.EDU",
            FullName = "System Administrator",
            PasswordHash = passwordHasher.HashPassword("Admin123!"),
            Role = UserRoles.Admin,
            IsActive = true,
            CreatedAt = now
        };

        var studentUser1 = new ApplicationUser
        {
            Email = "aidar.nurlanov@student.crm.edu",
            NormalizedEmail = "AIDAR.NURLANOV@STUDENT.CRM.EDU",
            FullName = "Aidar Nurlanov",
            PasswordHash = passwordHasher.HashPassword("Student123!"),
            Role = UserRoles.Student,
            IsActive = true,
            CreatedAt = now
        };

        var studentUser2 = new ApplicationUser
        {
            Email = "alina.kassymova@student.crm.edu",
            NormalizedEmail = "ALINA.KASSYMOVA@STUDENT.CRM.EDU",
            FullName = "Alina Kassymova",
            PasswordHash = passwordHasher.HashPassword("Student123!"),
            Role = UserRoles.Student,
            IsActive = true,
            CreatedAt = now
        };

        var studentUser3 = new ApplicationUser
        {
            Email = "timur.akhmetov@student.crm.edu",
            NormalizedEmail = "TIMUR.AKHMETOV@STUDENT.CRM.EDU",
            FullName = "Timur Akhmetov",
            PasswordHash = passwordHasher.HashPassword("Student123!"),
            Role = UserRoles.Student,
            IsActive = true,
            CreatedAt = now
        };

        var studentUser4 = new ApplicationUser
        {
            Email = "dana.suleimenova@student.crm.edu",
            NormalizedEmail = "DANA.SULEIMENOVA@STUDENT.CRM.EDU",
            FullName = "Dana Suleimenova",
            PasswordHash = passwordHasher.HashPassword("Student123!"),
            Role = UserRoles.Student,
            IsActive = true,
            CreatedAt = now
        };

        dbContext.Users.AddRange(adminUser, studentUser1, studentUser2, studentUser3, studentUser4);
        await dbContext.SaveChangesAsync(cancellationToken);

        var students = new List<Student>
        {
            new()
            {
                UserId = studentUser1.Id,
                StudentNumber = "STU-2026-0001",
                FullName = "Aidar Nurlanov",
                Email = studentUser1.Email,
                PhoneNumber = "+7 (701) 555-0142",
                DateOfBirth = new DateOnly(2005, 4, 15),
                University = "Astana IT University",
                Faculty = "School of Software Engineering",
                Department = "Computer Science & Engineering",
                StudyProgram = "B.Sc. in Software Engineering",
                StudyYear = 3,
                Bio = "Full-stack C#/.NET and distributed systems enthusiast. Lead developer of the University Student CRM project.",
                Status = StudentStatus.Active,
                EnrollmentDate = new DateOnly(2024, 9, 1),
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                UserId = studentUser2.Id,
                StudentNumber = "STU-2026-0002",
                FullName = "Alina Kassymova",
                Email = studentUser2.Email,
                PhoneNumber = "+7 (702) 555-0198",
                DateOfBirth = new DateOnly(2006, 8, 22),
                University = "Astana IT University",
                Faculty = "School of Data Science & AI",
                Department = "Artificial Intelligence & Big Data",
                StudyProgram = "B.Sc. in Data Science",
                StudyYear = 2,
                Bio = "Researching machine learning pipelines, academic analytics, and interactive web visualizations.",
                Status = StudentStatus.Active,
                EnrollmentDate = new DateOnly(2025, 9, 1),
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                UserId = studentUser3.Id,
                StudentNumber = "STU-2026-0003",
                FullName = "Timur Akhmetov",
                Email = studentUser3.Email,
                PhoneNumber = "+7 (705) 555-0311",
                DateOfBirth = new DateOnly(2005, 11, 3),
                University = "Astana IT University",
                Faculty = "School of Cyber Security",
                Department = "Information Security",
                StudyProgram = "B.Sc. in Cyber Security",
                StudyYear = 3,
                Bio = "Focusing on cloud security, cryptography, and secure API design.",
                Status = StudentStatus.Active,
                EnrollmentDate = new DateOnly(2024, 9, 1),
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                UserId = studentUser4.Id,
                StudentNumber = "STU-2026-0004",
                FullName = "Dana Suleimenova",
                Email = studentUser4.Email,
                PhoneNumber = "+7 (707) 555-0480",
                DateOfBirth = new DateOnly(2007, 2, 19),
                University = "Astana IT University",
                Faculty = "School of Software Engineering",
                Department = "Computer Science & Engineering",
                StudyProgram = "B.Sc. in Software Engineering",
                StudyYear = 1,
                Bio = "First-year software engineering student interested in UI/UX and web technologies.",
                Status = StudentStatus.Active,
                EnrollmentDate = new DateOnly(2026, 9, 1),
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                UserId = null,
                StudentNumber = "STU-2026-0005",
                FullName = "Miras Bekzhanov",
                Email = "miras.bekzhanov@student.crm.edu",
                PhoneNumber = "+7 (708) 555-0625",
                DateOfBirth = new DateOnly(2004, 6, 10),
                University = "Astana IT University",
                Faculty = "School of Digital Management",
                Department = "IT Management & Product Design",
                StudyProgram = "B.Sc. in IT Management",
                StudyYear = 4,
                Bio = "Senior year student on academic exchange semester.",
                Status = StudentStatus.Inactive,
                EnrollmentDate = new DateOnly(2023, 9, 1),
                CreatedAt = now,
                UpdatedAt = now
            }
        };

        dbContext.Students.AddRange(students);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger?.LogInformation("Seeded initial CRM users and {Count} student profiles.", students.Count);
    }
}
