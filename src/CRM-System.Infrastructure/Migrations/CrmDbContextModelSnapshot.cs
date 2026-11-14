using System;
using CRM_System.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace CRM_System.Infrastructure.Migrations;

[DbContext(typeof(CrmDbContext))]
partial class CrmDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        ConfigureSnapshotModel(modelBuilder);
    }

    internal static void ConfigureSnapshotModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder.HasAnnotation("ProductVersion", "10.0.0");

        modelBuilder.Entity("CRM_System.Core.Students.Models.ApplicationUser", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
            b.Property<DateTime>("CreatedAt").HasColumnType("TEXT");
            b.Property<string>("Email").IsRequired().HasMaxLength(160).HasColumnType("TEXT");
            b.Property<string>("FullName").IsRequired().HasMaxLength(120).HasColumnType("TEXT");
            b.Property<bool>("IsActive").HasColumnType("INTEGER");
            b.Property<DateTime?>("LastLoginAt").HasColumnType("TEXT");
            b.Property<string>("NormalizedEmail").IsRequired().HasMaxLength(160).HasColumnType("TEXT");
            b.Property<string>("PasswordHash").IsRequired().HasMaxLength(512).HasColumnType("TEXT");
            b.Property<string>("Role").IsRequired().HasMaxLength(32).HasColumnType("TEXT");
            b.HasKey("Id");
            b.HasIndex("Email").IsUnique();
            b.HasIndex("NormalizedEmail").IsUnique();
            b.ToTable("Users");
        });

        modelBuilder.Entity("CRM_System.Core.Students.Models.Student", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
            b.Property<string>("Bio").HasMaxLength(1000).HasColumnType("TEXT");
            b.Property<DateTime>("CreatedAt").HasColumnType("TEXT");
            b.Property<DateOnly?>("DateOfBirth").HasColumnType("TEXT");
            b.Property<string>("Department").IsRequired().HasMaxLength(120).HasColumnType("TEXT");
            b.Property<string>("Email").IsRequired().HasMaxLength(160).HasColumnType("TEXT");
            b.Property<DateOnly>("EnrollmentDate").HasColumnType("TEXT");
            b.Property<string>("Faculty").IsRequired().HasMaxLength(120).HasColumnType("TEXT");
            b.Property<string>("FullName").IsRequired().HasMaxLength(120).HasColumnType("TEXT");
            b.Property<string>("PhoneNumber").HasMaxLength(32).HasColumnType("TEXT");
            b.Property<int>("Status").HasColumnType("INTEGER");
            b.Property<string>("StudentNumber").IsRequired().HasMaxLength(32).HasColumnType("TEXT");
            b.Property<string>("StudyProgram").IsRequired().HasMaxLength(120).HasColumnType("TEXT");
            b.Property<int>("StudyYear").HasColumnType("INTEGER");
            b.Property<string>("University").IsRequired().HasMaxLength(150).HasColumnType("TEXT");
            b.Property<DateTime>("UpdatedAt").HasColumnType("TEXT");
            b.Property<int?>("UserId").HasColumnType("INTEGER");
            b.HasKey("Id");
            b.HasIndex("Email").IsUnique();
            b.HasIndex("StudentNumber").IsUnique();
            b.HasIndex("UserId").IsUnique();
            b.HasIndex("Faculty", "StudyYear", "Status");
            b.ToTable("Students");
        });

        modelBuilder.Entity("CRM_System.Core.Academic.Models.Teacher", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
            b.Property<string>("AcademicTitle").IsRequired().HasMaxLength(80).HasColumnType("TEXT");
            b.Property<string>("Bio").HasMaxLength(1000).HasColumnType("TEXT");
            b.Property<DateTime>("CreatedAt").HasColumnType("TEXT");
            b.Property<string>("Department").IsRequired().HasMaxLength(120).HasColumnType("TEXT");
            b.Property<string>("Email").IsRequired().HasMaxLength(160).HasColumnType("TEXT");
            b.Property<string>("FullName").IsRequired().HasMaxLength(120).HasColumnType("TEXT");
            b.Property<string>("OfficeRoom").HasMaxLength(40).HasColumnType("TEXT");
            b.Property<string>("PhoneNumber").HasMaxLength(32).HasColumnType("TEXT");
            b.HasKey("Id");
            b.HasIndex("Email").IsUnique();
            b.ToTable("Teachers");
        });

        modelBuilder.Entity("CRM_System.Core.Academic.Models.Course", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
            b.Property<string>("CourseCode").IsRequired().HasMaxLength(24).HasColumnType("TEXT");
            b.Property<DateTime>("CreatedAt").HasColumnType("TEXT");
            b.Property<int>("Credits").HasColumnType("INTEGER");
            b.Property<string>("Department").IsRequired().HasMaxLength(120).HasColumnType("TEXT");
            b.Property<string>("Description").IsRequired().HasMaxLength(1000).HasColumnType("TEXT");
            b.Property<bool>("IsActive").HasColumnType("INTEGER");
            b.Property<string>("Semester").IsRequired().HasMaxLength(40).HasColumnType("TEXT");
            b.Property<int?>("TeacherId").HasColumnType("INTEGER");
            b.Property<string>("Title").IsRequired().HasMaxLength(150).HasColumnType("TEXT");
            b.HasKey("Id");
            b.HasIndex("CourseCode").IsUnique();
            b.HasIndex("TeacherId");
            b.ToTable("Courses");
        });

        modelBuilder.Entity("CRM_System.Core.Academic.Models.Enrollment", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
            b.Property<int>("CourseId").HasColumnType("INTEGER");
            b.Property<DateTime>("EnrolledAt").HasColumnType("TEXT");
            b.Property<string>("Semester").IsRequired().HasMaxLength(40).HasColumnType("TEXT");
            b.Property<int>("Status").HasColumnType("INTEGER");
            b.Property<int>("StudentId").HasColumnType("INTEGER");
            b.HasKey("Id");
            b.HasIndex("CourseId");
            b.HasIndex("StudentId", "CourseId").IsUnique();
            b.ToTable("Enrollments");
        });

        modelBuilder.Entity("CRM_System.Core.Academic.Models.Grade", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
            b.Property<string>("AssessmentTitle").IsRequired().HasMaxLength(120).HasColumnType("TEXT");
            b.Property<string>("AssessmentType").IsRequired().HasMaxLength(40).HasColumnType("TEXT");
            b.Property<int>("CourseId").HasColumnType("INTEGER");
            b.Property<string>("Feedback").HasMaxLength(500).HasColumnType("TEXT");
            b.Property<DateTime>("GradedAt").HasColumnType("TEXT");
            b.Property<decimal>("Score").HasPrecision(5, 2).HasColumnType("TEXT");
            b.Property<int>("StudentId").HasColumnType("INTEGER");
            b.Property<decimal>("Weight").HasPrecision(4, 2).HasColumnType("TEXT");
            b.HasKey("Id");
            b.HasIndex("CourseId");
            b.HasIndex("StudentId", "CourseId");
            b.ToTable("Grades");
        });

        modelBuilder.Entity("CRM_System.Core.Academic.Models.ScheduleEntry", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
            b.Property<string>("Classroom").IsRequired().HasMaxLength(60).HasColumnType("TEXT");
            b.Property<int>("CourseId").HasColumnType("INTEGER");
            b.Property<int>("DayOfWeek").HasColumnType("INTEGER");
            b.Property<TimeOnly>("EndTime").HasColumnType("TEXT");
            b.Property<string>("LessonType").IsRequired().HasMaxLength(40).HasColumnType("TEXT");
            b.Property<TimeOnly>("StartTime").HasColumnType("TEXT");
            b.HasKey("Id");
            b.HasIndex("CourseId");
            b.HasIndex("DayOfWeek", "StartTime");
            b.ToTable("ScheduleEntries");
        });

        modelBuilder.Entity("CRM_System.Core.Academic.Models.AttendanceRecord", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
            b.Property<int>("CourseId").HasColumnType("INTEGER");
            b.Property<DateOnly>("Date").HasColumnType("TEXT");
            b.Property<string>("Notes").HasMaxLength(500).HasColumnType("TEXT");
            b.Property<DateTime>("RecordedAt").HasColumnType("TEXT");
            b.Property<int>("Status").HasColumnType("INTEGER");
            b.Property<int>("StudentId").HasColumnType("INTEGER");
            b.Property<string>("Topic").HasMaxLength(150).HasColumnType("TEXT");
            b.HasKey("Id");
            b.HasIndex("CourseId");
            b.HasIndex("StudentId", "CourseId", "Date");
            b.ToTable("AttendanceRecords");
        });

        modelBuilder.Entity("CRM_System.Core.Students.Models.Student", b =>
        {
            b.HasOne("CRM_System.Core.Students.Models.ApplicationUser", "User")
                .WithOne("StudentProfile")
                .HasForeignKey("CRM_System.Core.Students.Models.Student", "UserId")
                .OnDelete(DeleteBehavior.SetNull);
            b.Navigation("User");
        });

        modelBuilder.Entity("CRM_System.Core.Academic.Models.Course", b =>
        {
            b.HasOne("CRM_System.Core.Academic.Models.Teacher", "Teacher")
                .WithMany("Courses")
                .HasForeignKey("TeacherId")
                .OnDelete(DeleteBehavior.SetNull);
            b.Navigation("Teacher");
        });

        modelBuilder.Entity("CRM_System.Core.Academic.Models.Enrollment", b =>
        {
            b.HasOne("CRM_System.Core.Academic.Models.Course", "Course")
                .WithMany("Enrollments")
                .HasForeignKey("CourseId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.HasOne("CRM_System.Core.Students.Models.Student", "Student")
                .WithMany()
                .HasForeignKey("StudentId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.Navigation("Course");
            b.Navigation("Student");
        });

        modelBuilder.Entity("CRM_System.Core.Academic.Models.Grade", b =>
        {
            b.HasOne("CRM_System.Core.Academic.Models.Course", "Course")
                .WithMany("Grades")
                .HasForeignKey("CourseId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.HasOne("CRM_System.Core.Students.Models.Student", "Student")
                .WithMany()
                .HasForeignKey("StudentId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.Navigation("Course");
            b.Navigation("Student");
        });

        modelBuilder.Entity("CRM_System.Core.Academic.Models.ScheduleEntry", b =>
        {
            b.HasOne("CRM_System.Core.Academic.Models.Course", "Course")
                .WithMany("ScheduleEntries")
                .HasForeignKey("CourseId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.Navigation("Course");
        });

        modelBuilder.Entity("CRM_System.Core.Academic.Models.AttendanceRecord", b =>
        {
            b.HasOne("CRM_System.Core.Academic.Models.Course", "Course")
                .WithMany("AttendanceRecords")
                .HasForeignKey("CourseId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.HasOne("CRM_System.Core.Students.Models.Student", "Student")
                .WithMany()
                .HasForeignKey("StudentId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.Navigation("Course");
            b.Navigation("Student");
        });

        modelBuilder.Entity("CRM_System.Core.Students.Models.ApplicationUser", b =>
        {
            b.Navigation("StudentProfile");
        });

        modelBuilder.Entity("CRM_System.Core.Academic.Models.Teacher", b =>
        {
            b.Navigation("Courses");
        });

        modelBuilder.Entity("CRM_System.Core.Academic.Models.Course", b =>
        {
            b.Navigation("AttendanceRecords");
            b.Navigation("Enrollments");
            b.Navigation("Grades");
            b.Navigation("ScheduleEntries");
        });
#pragma warning restore 612, 618
    }
}
