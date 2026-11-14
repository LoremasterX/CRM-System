using CRM_System.Infrastructure.Data;
using CRM_System.Infrastructure.Students;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CRM_System.Tests.Academic;

public class AcademicDatabaseMigrationTests
{
    [Fact]
    public async Task MigrateAndSeedAcademicData_SqliteDatabase_CreatesAllTablesAndSeedsAcademicCatalogIdempotently()
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
        // Second call must be idempotent
        await DatabaseInitializer.SeedAcademicDataAsync(db);

        Assert.Equal(4, await db.Teachers.CountAsync());
        Assert.Equal(5, await db.Courses.CountAsync());
        Assert.Equal(9, await db.Enrollments.CountAsync());
        Assert.Equal(9, await db.Grades.CountAsync());
        Assert.Equal(8, await db.ScheduleEntries.CountAsync());
        Assert.Equal(10, await db.AttendanceRecords.CountAsync());
    }
}
