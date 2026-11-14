using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM_System.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddAcademicManagement : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Teachers",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                FullName = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                Email = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                Department = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                AcademicTitle = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                OfficeRoom = table.Column<string>(type: "TEXT", maxLength: 40, nullable: true),
                PhoneNumber = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                Bio = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Teachers", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Courses",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                CourseCode = table.Column<string>(type: "TEXT", maxLength: 24, nullable: false),
                Title = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                Credits = table.Column<int>(type: "INTEGER", nullable: false),
                Department = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                Semester = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                TeacherId = table.Column<int>(type: "INTEGER", nullable: true),
                IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Courses", x => x.Id);
                table.ForeignKey(
                    name: "FK_Courses_Teachers_TeacherId",
                    column: x => x.TeacherId,
                    principalTable: "Teachers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "Enrollments",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                StudentId = table.Column<int>(type: "INTEGER", nullable: false),
                CourseId = table.Column<int>(type: "INTEGER", nullable: false),
                Semester = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                Status = table.Column<int>(type: "INTEGER", nullable: false),
                EnrolledAt = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Enrollments", x => x.Id);
                table.ForeignKey(
                    name: "FK_Enrollments_Courses_CourseId",
                    column: x => x.CourseId,
                    principalTable: "Courses",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Enrollments_Students_StudentId",
                    column: x => x.StudentId,
                    principalTable: "Students",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Grades",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                StudentId = table.Column<int>(type: "INTEGER", nullable: false),
                CourseId = table.Column<int>(type: "INTEGER", nullable: false),
                AssessmentTitle = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                AssessmentType = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                Score = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                Weight = table.Column<decimal>(type: "TEXT", precision: 4, scale: 2, nullable: false),
                Feedback = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                GradedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Grades", x => x.Id);
                table.ForeignKey(
                    name: "FK_Grades_Courses_CourseId",
                    column: x => x.CourseId,
                    principalTable: "Courses",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Grades_Students_StudentId",
                    column: x => x.StudentId,
                    principalTable: "Students",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ScheduleEntries",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                CourseId = table.Column<int>(type: "INTEGER", nullable: false),
                DayOfWeek = table.Column<int>(type: "INTEGER", nullable: false),
                StartTime = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                EndTime = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                Classroom = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                LessonType = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ScheduleEntries", x => x.Id);
                table.ForeignKey(
                    name: "FK_ScheduleEntries_Courses_CourseId",
                    column: x => x.CourseId,
                    principalTable: "Courses",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AttendanceRecords",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                StudentId = table.Column<int>(type: "INTEGER", nullable: false),
                CourseId = table.Column<int>(type: "INTEGER", nullable: false),
                Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                Status = table.Column<int>(type: "INTEGER", nullable: false),
                Topic = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                RecordedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AttendanceRecords", x => x.Id);
                table.ForeignKey(
                    name: "FK_AttendanceRecords_Courses_CourseId",
                    column: x => x.CourseId,
                    principalTable: "Courses",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_AttendanceRecords_Students_StudentId",
                    column: x => x.StudentId,
                    principalTable: "Students",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Teachers_Email",
            table: "Teachers",
            column: "Email",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Courses_CourseCode",
            table: "Courses",
            column: "CourseCode",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Courses_TeacherId",
            table: "Courses",
            column: "TeacherId");

        migrationBuilder.CreateIndex(
            name: "IX_Enrollments_CourseId",
            table: "Enrollments",
            column: "CourseId");

        migrationBuilder.CreateIndex(
            name: "IX_Enrollments_StudentId_CourseId",
            table: "Enrollments",
            columns: new[] { "StudentId", "CourseId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Grades_CourseId",
            table: "Grades",
            column: "CourseId");

        migrationBuilder.CreateIndex(
            name: "IX_Grades_StudentId_CourseId",
            table: "Grades",
            columns: new[] { "StudentId", "CourseId" });

        migrationBuilder.CreateIndex(
            name: "IX_ScheduleEntries_CourseId",
            table: "ScheduleEntries",
            column: "CourseId");

        migrationBuilder.CreateIndex(
            name: "IX_ScheduleEntries_DayOfWeek_StartTime",
            table: "ScheduleEntries",
            columns: new[] { "DayOfWeek", "StartTime" });

        migrationBuilder.CreateIndex(
            name: "IX_AttendanceRecords_CourseId",
            table: "AttendanceRecords",
            column: "CourseId");

        migrationBuilder.CreateIndex(
            name: "IX_AttendanceRecords_StudentId_CourseId_Date",
            table: "AttendanceRecords",
            columns: new[] { "StudentId", "CourseId", "Date" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "AttendanceRecords");
        migrationBuilder.DropTable(name: "Enrollments");
        migrationBuilder.DropTable(name: "Grades");
        migrationBuilder.DropTable(name: "ScheduleEntries");
        migrationBuilder.DropTable(name: "Courses");
        migrationBuilder.DropTable(name: "Teachers");
    }
}
