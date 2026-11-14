using CRM_System.Core.Academic.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM_System.Infrastructure.Data.Configurations;

public class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
{
    public void Configure(EntityTypeBuilder<Teacher> builder)
    {
        builder.ToTable("Teachers");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.FullName).IsRequired().HasMaxLength(120);
        builder.Property(t => t.Email).IsRequired().HasMaxLength(160);
        builder.Property(t => t.Department).IsRequired().HasMaxLength(120);
        builder.Property(t => t.AcademicTitle).IsRequired().HasMaxLength(80);
        builder.Property(t => t.OfficeRoom).HasMaxLength(40);
        builder.Property(t => t.PhoneNumber).HasMaxLength(32);
        builder.Property(t => t.Bio).HasMaxLength(1000);

        builder.HasIndex(t => t.Email).IsUnique();
    }
}

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("Courses");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.CourseCode).IsRequired().HasMaxLength(24);
        builder.Property(c => c.Title).IsRequired().HasMaxLength(150);
        builder.Property(c => c.Description).IsRequired().HasMaxLength(1000);
        builder.Property(c => c.Department).IsRequired().HasMaxLength(120);
        builder.Property(c => c.Semester).IsRequired().HasMaxLength(40);

        builder.HasIndex(c => c.CourseCode).IsUnique();

        builder.HasOne(c => c.Teacher)
            .WithMany(t => t.Courses)
            .HasForeignKey(c => c.TeacherId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.ToTable("Enrollments");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Semester).IsRequired().HasMaxLength(40);
        builder.Property(e => e.Status).HasConversion<int>();

        builder.HasIndex(e => new { e.StudentId, e.CourseId }).IsUnique();

        builder.HasOne(e => e.Student)
            .WithMany()
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Course)
            .WithMany(c => c.Enrollments)
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class GradeConfiguration : IEntityTypeConfiguration<Grade>
{
    public void Configure(EntityTypeBuilder<Grade> builder)
    {
        builder.ToTable("Grades");
        builder.HasKey(g => g.Id);

        builder.Property(g => g.AssessmentTitle).IsRequired().HasMaxLength(120);
        builder.Property(g => g.AssessmentType).IsRequired().HasMaxLength(40);
        builder.Property(g => g.Score).HasPrecision(5, 2);
        builder.Property(g => g.Weight).HasPrecision(4, 2);
        builder.Property(g => g.Feedback).HasMaxLength(500);

        builder.HasIndex(g => new { g.StudentId, g.CourseId });

        builder.HasOne(g => g.Student)
            .WithMany()
            .HasForeignKey(g => g.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(g => g.Course)
            .WithMany(c => c.Grades)
            .HasForeignKey(g => g.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ScheduleEntryConfiguration : IEntityTypeConfiguration<ScheduleEntry>
{
    public void Configure(EntityTypeBuilder<ScheduleEntry> builder)
    {
        builder.ToTable("ScheduleEntries");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.DayOfWeek).HasConversion<int>();
        builder.Property(s => s.Classroom).IsRequired().HasMaxLength(60);
        builder.Property(s => s.LessonType).IsRequired().HasMaxLength(40);

        builder.HasIndex(s => new { s.DayOfWeek, s.StartTime });

        builder.HasOne(s => s.Course)
            .WithMany(c => c.ScheduleEntries)
            .HasForeignKey(s => s.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class AttendanceRecordConfiguration : IEntityTypeConfiguration<AttendanceRecord>
{
    public void Configure(EntityTypeBuilder<AttendanceRecord> builder)
    {
        builder.ToTable("AttendanceRecords");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Status).HasConversion<int>();
        builder.Property(a => a.Topic).HasMaxLength(150);
        builder.Property(a => a.Notes).HasMaxLength(500);

        builder.HasIndex(a => new { a.StudentId, a.CourseId, a.Date });

        builder.HasOne(a => a.Student)
            .WithMany()
            .HasForeignKey(a => a.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Course)
            .WithMany(c => c.AttendanceRecords)
            .HasForeignKey(a => a.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
