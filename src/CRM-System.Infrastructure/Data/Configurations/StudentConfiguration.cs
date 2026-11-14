using CRM_System.Core.Students.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM_System.Infrastructure.Data.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.StudentNumber)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(s => s.FullName)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(s => s.Email)
            .IsRequired()
            .HasMaxLength(160);

        builder.Property(s => s.PhoneNumber)
            .HasMaxLength(32);

        builder.Property(s => s.University)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(s => s.Faculty)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(s => s.Department)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(s => s.StudyProgram)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(s => s.Bio)
            .HasMaxLength(1000);

        builder.Property(s => s.Status)
            .HasConversion<int>();

        builder.HasIndex(s => s.StudentNumber)
            .IsUnique();

        builder.HasIndex(s => s.Email)
            .IsUnique();

        builder.HasIndex(s => s.UserId)
            .IsUnique();

        builder.HasIndex(s => new { s.Faculty, s.StudyYear, s.Status });

        builder.HasOne(s => s.User)
            .WithOne(u => u.StudentProfile)
            .HasForeignKey<Student>(s => s.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Ignore(s => s.IsActive);
        builder.Ignore(s => s.Initials);
    }
}
