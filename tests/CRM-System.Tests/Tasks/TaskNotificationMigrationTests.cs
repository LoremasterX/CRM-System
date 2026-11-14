using CRM_System.Infrastructure.Data;
using CRM_System.Infrastructure.Students;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CRM_System.Tests.Tasks;

public class TaskNotificationMigrationTests
{
    [Fact]
    public async Task MigrateAndSeedTasksAndNotifications_SqliteDatabase_AppliesAllThreeStageMigrationsAndSeedsIdempotently()
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
        await DatabaseInitializer.SeedAcademicDataAsync(db);
        await DatabaseInitializer.SeedTasksAndNotificationsAsync(db);
        // Second call must be idempotent
        await DatabaseInitializer.SeedTasksAndNotificationsAsync(db);

        Assert.Equal(8, await db.StudentTasks.CountAsync());
        Assert.Equal(5, await db.Notifications.CountAsync());
    }
}
