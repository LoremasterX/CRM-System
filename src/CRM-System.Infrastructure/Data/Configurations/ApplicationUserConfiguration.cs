using CRM_System.Core.Students.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM_System.Infrastructure.Data.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(160);

        builder.Property(u => u.NormalizedEmail)
            .IsRequired()
            .HasMaxLength(160);

        builder.Property(u => u.FullName)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(u => u.Role)
            .IsRequired()
            .HasMaxLength(32);

        builder.HasIndex(u => u.NormalizedEmail)
            .IsUnique();

        builder.HasIndex(u => u.Email)
            .IsUnique();
    }
}
