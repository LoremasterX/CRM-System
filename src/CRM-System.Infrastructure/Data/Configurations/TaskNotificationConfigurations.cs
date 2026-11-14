using CRM_System.Core.Tasks.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM_System.Infrastructure.Data.Configurations;

public class StudentTaskConfiguration : IEntityTypeConfiguration<StudentTask>
{
    public void Configure(EntityTypeBuilder<StudentTask> builder)
    {
        builder.ToTable("StudentTasks");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.Priority)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.HasIndex(x => new { x.StudentId, x.Status, x.DueDate });
        builder.HasIndex(x => x.CourseId);

        builder.HasOne(x => x.Student)
            .WithMany()
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Course)
            .WithMany()
            .HasForeignKey(x => x.CourseId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Message)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.Type)
            .IsRequired();

        builder.HasIndex(x => new { x.StudentId, x.IsRead, x.CreatedAt });
        builder.HasIndex(x => new { x.UserId, x.IsRead, x.CreatedAt });

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Student)
            .WithMany()
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.RelatedTask)
            .WithMany()
            .HasForeignKey(x => x.RelatedTaskId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.RelatedCourse)
            .WithMany()
            .HasForeignKey(x => x.RelatedCourseId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
