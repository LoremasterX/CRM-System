using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM_System.Infrastructure.Migrations;

public partial class AddTasksAndNotifications : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "StudentTasks",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                StudentId = table.Column<int>(type: "INTEGER", nullable: false),
                CourseId = table.Column<int>(type: "INTEGER", nullable: true),
                Title = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                DueDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                Priority = table.Column<int>(type: "INTEGER", nullable: false),
                Status = table.Column<int>(type: "INTEGER", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StudentTasks", x => x.Id);
                table.ForeignKey(
                    name: "FK_StudentTasks_Courses_CourseId",
                    column: x => x.CourseId,
                    principalTable: "Courses",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_StudentTasks_Students_StudentId",
                    column: x => x.StudentId,
                    principalTable: "Students",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Notifications",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                UserId = table.Column<int>(type: "INTEGER", nullable: true),
                StudentId = table.Column<int>(type: "INTEGER", nullable: true),
                RelatedTaskId = table.Column<int>(type: "INTEGER", nullable: true),
                RelatedCourseId = table.Column<int>(type: "INTEGER", nullable: true),
                Title = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                Message = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                Type = table.Column<int>(type: "INTEGER", nullable: false),
                IsRead = table.Column<bool>(type: "INTEGER", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                ReadAt = table.Column<DateTime>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Notifications", x => x.Id);
                table.ForeignKey(
                    name: "FK_Notifications_Courses_RelatedCourseId",
                    column: x => x.RelatedCourseId,
                    principalTable: "Courses",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_Notifications_StudentTasks_RelatedTaskId",
                    column: x => x.RelatedTaskId,
                    principalTable: "StudentTasks",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_Notifications_Students_StudentId",
                    column: x => x.StudentId,
                    principalTable: "Students",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Notifications_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_StudentTasks_CourseId",
            table: "StudentTasks",
            column: "CourseId");

        migrationBuilder.CreateIndex(
            name: "IX_StudentTasks_StudentId_Status_DueDate",
            table: "StudentTasks",
            columns: new[] { "StudentId", "Status", "DueDate" });

        migrationBuilder.CreateIndex(
            name: "IX_Notifications_RelatedCourseId",
            table: "Notifications",
            column: "RelatedCourseId");

        migrationBuilder.CreateIndex(
            name: "IX_Notifications_RelatedTaskId",
            table: "Notifications",
            column: "RelatedTaskId");

        migrationBuilder.CreateIndex(
            name: "IX_Notifications_StudentId_IsRead_CreatedAt",
            table: "Notifications",
            columns: new[] { "StudentId", "IsRead", "CreatedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_Notifications_UserId_IsRead_CreatedAt",
            table: "Notifications",
            columns: new[] { "UserId", "IsRead", "CreatedAt" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Notifications");

        migrationBuilder.DropTable(
            name: "StudentTasks");
    }
}
