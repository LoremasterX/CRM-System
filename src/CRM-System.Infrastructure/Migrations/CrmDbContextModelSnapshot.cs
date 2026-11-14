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
#pragma warning disable 612, 618
        modelBuilder.HasAnnotation("ProductVersion", "10.0.0");

        modelBuilder.Entity("CRM_System.Core.Students.Models.ApplicationUser", b =>
        {
            b.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("INTEGER");

            b.Property<DateTime>("CreatedAt")
                .HasColumnType("TEXT");

            b.Property<string>("Email")
                .IsRequired()
                .HasMaxLength(160)
                .HasColumnType("TEXT");

            b.Property<string>("FullName")
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnType("TEXT");

            b.Property<bool>("IsActive")
                .HasColumnType("INTEGER");

            b.Property<DateTime?>("LastLoginAt")
                .HasColumnType("TEXT");

            b.Property<string>("NormalizedEmail")
                .IsRequired()
                .HasMaxLength(160)
                .HasColumnType("TEXT");

            b.Property<string>("PasswordHash")
                .IsRequired()
                .HasMaxLength(512)
                .HasColumnType("TEXT");

            b.Property<string>("Role")
                .IsRequired()
                .HasMaxLength(32)
                .HasColumnType("TEXT");

            b.HasKey("Id");

            b.HasIndex("Email")
                .IsUnique();

            b.HasIndex("NormalizedEmail")
                .IsUnique();

            b.ToTable("Users");
        });

        modelBuilder.Entity("CRM_System.Core.Students.Models.Student", b =>
        {
            b.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("INTEGER");

            b.Property<string>("Bio")
                .HasMaxLength(1000)
                .HasColumnType("TEXT");

            b.Property<DateTime>("CreatedAt")
                .HasColumnType("TEXT");

            b.Property<DateOnly?>("DateOfBirth")
                .HasColumnType("TEXT");

            b.Property<string>("Department")
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnType("TEXT");

            b.Property<string>("Email")
                .IsRequired()
                .HasMaxLength(160)
                .HasColumnType("TEXT");

            b.Property<DateOnly>("EnrollmentDate")
                .HasColumnType("TEXT");

            b.Property<string>("Faculty")
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnType("TEXT");

            b.Property<string>("FullName")
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnType("TEXT");

            b.Property<string>("PhoneNumber")
                .HasMaxLength(32)
                .HasColumnType("TEXT");

            b.Property<int>("Status")
                .HasColumnType("INTEGER");

            b.Property<string>("StudentNumber")
                .IsRequired()
                .HasMaxLength(32)
                .HasColumnType("TEXT");

            b.Property<string>("StudyProgram")
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnType("TEXT");

            b.Property<int>("StudyYear")
                .HasColumnType("INTEGER");

            b.Property<string>("University")
                .IsRequired()
                .HasMaxLength(150)
                .HasColumnType("TEXT");

            b.Property<DateTime>("UpdatedAt")
                .HasColumnType("TEXT");

            b.Property<int?>("UserId")
                .HasColumnType("INTEGER");

            b.HasKey("Id");

            b.HasIndex("Email")
                .IsUnique();

            b.HasIndex("StudentNumber")
                .IsUnique();

            b.HasIndex("UserId")
                .IsUnique();

            b.HasIndex("Faculty", "StudyYear", "Status");

            b.ToTable("Students");
        });

        modelBuilder.Entity("CRM_System.Core.Students.Models.Student", b =>
        {
            b.HasOne("CRM_System.Core.Students.Models.ApplicationUser", "User")
                .WithOne("StudentProfile")
                .HasForeignKey("CRM_System.Core.Students.Models.Student", "UserId")
                .OnDelete(DeleteBehavior.SetNull);

            b.Navigation("User");
        });

        modelBuilder.Entity("CRM_System.Core.Students.Models.ApplicationUser", b =>
        {
            b.Navigation("StudentProfile");
        });
#pragma warning restore 612, 618
    }
}
