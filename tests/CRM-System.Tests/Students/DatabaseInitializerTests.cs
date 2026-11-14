using CRM_System.Core.Students.Models;
using CRM_System.Infrastructure.Data;
using CRM_System.Infrastructure.Students;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CRM_System.Tests.Students;

public class DatabaseInitializerTests
{
    [Fact]
    public async Task MigrateAndSeed_SqliteDatabase_AppliesMigrationAndSeedsIdempotently()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new CrmDbContext(options);
        var hasher = new PasswordHasherAdapter();

        await db.Database.MigrateAsync();
        await DatabaseInitializer.SeedInitialUsersAndStudentsAsync(db, hasher);
        // Second call must be idempotent and not duplicate records
        await DatabaseInitializer.SeedInitialUsersAndStudentsAsync(db, hasher);

        var users = await db.Users.ToListAsync();
        var students = await db.Students.ToListAsync();

        Assert.Equal(5, users.Count);
        Assert.Equal(5, students.Count);
        Assert.Single(users, u => u.Role == UserRoles.Admin && u.Email == "admin@crm.edu");
        Assert.Contains(students, s => s.StudentNumber == "STU-2026-0001" && s.FullName == "Aidar Nurlanov");
    }
}
