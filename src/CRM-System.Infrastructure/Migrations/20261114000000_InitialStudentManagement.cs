using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM_System.Infrastructure.Migrations;

/// <inheritdoc />
public partial class InitialStudentManagement : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Email = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                NormalizedEmail = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                FullName = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                PasswordHash = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                Role = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                LastLoginAt = table.Column<DateTime>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Students",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                UserId = table.Column<int>(type: "INTEGER", nullable: true),
                StudentNumber = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                FullName = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                Email = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                PhoneNumber = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                DateOfBirth = table.Column<DateOnly>(type: "TEXT", nullable: true),
                University = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                Faculty = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                Department = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                StudyProgram = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                StudyYear = table.Column<int>(type: "INTEGER", nullable: false),
                Bio = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                Status = table.Column<int>(type: "INTEGER", nullable: false),
                EnrollmentDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Students", x => x.Id);
                table.ForeignKey(
                    name: "FK_Students_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Users_Email",
            table: "Users",
            column: "Email",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Users_NormalizedEmail",
            table: "Users",
            column: "NormalizedEmail",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Students_Email",
            table: "Students",
            column: "Email",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Students_Faculty_StudyYear_Status",
            table: "Students",
            columns: new[] { "Faculty", "StudyYear", "Status" });

        migrationBuilder.CreateIndex(
            name: "IX_Students_StudentNumber",
            table: "Students",
            column: "StudentNumber",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Students_UserId",
            table: "Students",
            column: "UserId",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Students");
        migrationBuilder.DropTable(name: "Users");
    }
}
