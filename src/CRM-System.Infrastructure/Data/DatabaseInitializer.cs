using CRM_System.Core.Academic.Models;
using CRM_System.Core.Students.Interfaces;
using CRM_System.Core.Students.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CRM_System.Infrastructure.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeCrmDatabaseAsync(
        this IServiceProvider services,
        bool seedDemoData = true,
        CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var logger = scope.ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger("DatabaseInitializer");
        var dbContext = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        if (dbContext.Database.IsRelational())
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        }
        else
        {
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        }

        if (seedDemoData)
        {
            await SeedInitialUsersAndStudentsAsync(dbContext, passwordHasher, logger, cancellationToken);
            await SeedAcademicDataAsync(dbContext, logger, cancellationToken);
        }
    }

    public static async Task SeedInitialUsersAndStudentsAsync(
        CrmDbContext dbContext,
        IPasswordHasher passwordHasher,
        ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        if (await dbContext.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = DateTime.UtcNow;

        var adminUser = new ApplicationUser
        {
            Email = "admin@crm.edu",
            NormalizedEmail = "ADMIN@CRM.EDU",
            FullName = "System Administrator",
            PasswordHash = passwordHasher.HashPassword("Admin123!"),
            Role = UserRoles.Admin,
            IsActive = true,
            CreatedAt = now
        };

        var studentUser1 = new ApplicationUser
        {
            Email = "aidar.nurlanov@student.crm.edu",
            NormalizedEmail = "AIDAR.NURLANOV@STUDENT.CRM.EDU",
            FullName = "Aidar Nurlanov",
            PasswordHash = passwordHasher.HashPassword("Student123!"),
            Role = UserRoles.Student,
            IsActive = true,
            CreatedAt = now
        };

        var studentUser2 = new ApplicationUser
        {
            Email = "alina.kassymova@student.crm.edu",
            NormalizedEmail = "ALINA.KASSYMOVA@STUDENT.CRM.EDU",
            FullName = "Alina Kassymova",
            PasswordHash = passwordHasher.HashPassword("Student123!"),
            Role = UserRoles.Student,
            IsActive = true,
            CreatedAt = now
        };

        var studentUser3 = new ApplicationUser
        {
            Email = "timur.akhmetov@student.crm.edu",
            NormalizedEmail = "TIMUR.AKHMETOV@STUDENT.CRM.EDU",
            FullName = "Timur Akhmetov",
            PasswordHash = passwordHasher.HashPassword("Student123!"),
            Role = UserRoles.Student,
            IsActive = true,
            CreatedAt = now
        };

        var studentUser4 = new ApplicationUser
        {
            Email = "dana.suleimenova@student.crm.edu",
            NormalizedEmail = "DANA.SULEIMENOVA@STUDENT.CRM.EDU",
            FullName = "Dana Suleimenova",
            PasswordHash = passwordHasher.HashPassword("Student123!"),
            Role = UserRoles.Student,
            IsActive = true,
            CreatedAt = now
        };

        dbContext.Users.AddRange(adminUser, studentUser1, studentUser2, studentUser3, studentUser4);
        await dbContext.SaveChangesAsync(cancellationToken);

        var students = new List<Student>
        {
            new()
            {
                UserId = studentUser1.Id,
                StudentNumber = "STU-2026-0001",
                FullName = "Aidar Nurlanov",
                Email = studentUser1.Email,
                PhoneNumber = "+7 (701) 555-0142",
                DateOfBirth = new DateOnly(2005, 4, 15),
                University = "Astana IT University",
                Faculty = "School of Software Engineering",
                Department = "Computer Science & Engineering",
                StudyProgram = "B.Sc. in Software Engineering",
                StudyYear = 3,
                Bio = "Full-stack C#/.NET and distributed systems enthusiast. Lead developer of the University Student CRM project.",
                Status = StudentStatus.Active,
                EnrollmentDate = new DateOnly(2024, 9, 1),
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                UserId = studentUser2.Id,
                StudentNumber = "STU-2026-0002",
                FullName = "Alina Kassymova",
                Email = studentUser2.Email,
                PhoneNumber = "+7 (702) 555-0198",
                DateOfBirth = new DateOnly(2006, 8, 22),
                University = "Astana IT University",
                Faculty = "School of Data Science & AI",
                Department = "Artificial Intelligence & Big Data",
                StudyProgram = "B.Sc. in Data Science",
                StudyYear = 2,
                Bio = "Researching machine learning pipelines, academic analytics, and interactive web visualizations.",
                Status = StudentStatus.Active,
                EnrollmentDate = new DateOnly(2025, 9, 1),
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                UserId = studentUser3.Id,
                StudentNumber = "STU-2026-0003",
                FullName = "Timur Akhmetov",
                Email = studentUser3.Email,
                PhoneNumber = "+7 (705) 555-0311",
                DateOfBirth = new DateOnly(2005, 11, 3),
                University = "Astana IT University",
                Faculty = "School of Cyber Security",
                Department = "Information Security",
                StudyProgram = "B.Sc. in Cyber Security",
                StudyYear = 3,
                Bio = "Focusing on cloud security, cryptography, and secure API design.",
                Status = StudentStatus.Active,
                EnrollmentDate = new DateOnly(2024, 9, 1),
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                UserId = studentUser4.Id,
                StudentNumber = "STU-2026-0004",
                FullName = "Dana Suleimenova",
                Email = studentUser4.Email,
                PhoneNumber = "+7 (707) 555-0480",
                DateOfBirth = new DateOnly(2007, 2, 19),
                University = "Astana IT University",
                Faculty = "School of Software Engineering",
                Department = "Computer Science & Engineering",
                StudyProgram = "B.Sc. in Software Engineering",
                StudyYear = 1,
                Bio = "First-year software engineering student interested in UI/UX and web technologies.",
                Status = StudentStatus.Active,
                EnrollmentDate = new DateOnly(2026, 9, 1),
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                UserId = null,
                StudentNumber = "STU-2026-0005",
                FullName = "Miras Bekzhanov",
                Email = "miras.bekzhanov@student.crm.edu",
                PhoneNumber = "+7 (708) 555-0625",
                DateOfBirth = new DateOnly(2004, 6, 10),
                University = "Astana IT University",
                Faculty = "School of Digital Management",
                Department = "IT Management & Product Design",
                StudyProgram = "B.Sc. in IT Management",
                StudyYear = 4,
                Bio = "Senior year student on academic exchange semester.",
                Status = StudentStatus.Inactive,
                EnrollmentDate = new DateOnly(2023, 9, 1),
                CreatedAt = now,
                UpdatedAt = now
            }
        };

        dbContext.Students.AddRange(students);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger?.LogInformation("Seeded initial CRM users and {Count} student profiles.", students.Count);
    }

    public static async Task SeedAcademicDataAsync(
        CrmDbContext dbContext,
        ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        if (await dbContext.Courses.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = DateTime.UtcNow;

        var teachers = new List<Teacher>
        {
            new()
            {
                FullName = "Dr. Serikbolsyn Nurmagambetov",
                Email = "s.nurmagambetov@faculty.crm.edu",
                Department = "Computer Science & Engineering",
                AcademicTitle = "Professor",
                OfficeRoom = "C1.3.310",
                PhoneNumber = "+7 (7172) 645-701",
                Bio = "PhD in Software Architecture & Distributed Cloud Computing.",
                CreatedAt = now
            },
            new()
            {
                FullName = "Dr. Aizhan Baimukhanova",
                Email = "a.baimukhanova@faculty.crm.edu",
                Department = "Artificial Intelligence & Big Data",
                AcademicTitle = "Associate Professor",
                OfficeRoom = "C1.2.215",
                PhoneNumber = "+7 (7172) 645-702",
                Bio = "Lead researcher in Statistical Machine Learning and Data Engineering.",
                CreatedAt = now
            },
            new()
            {
                FullName = "Marat Iskakov",
                Email = "m.iskakov@faculty.crm.edu",
                Department = "Information Security",
                AcademicTitle = "Senior Lecturer",
                OfficeRoom = "C1.1.118",
                PhoneNumber = "+7 (7172) 645-703",
                Bio = "Applied Cryptography, Application Security, and Penetration Testing instructor.",
                CreatedAt = now
            },
            new()
            {
                FullName = "Dr. Gulnara Tazhibayeva",
                Email = "g.tazhibayeva@faculty.crm.edu",
                Department = "Computer Science & Engineering",
                AcademicTitle = "Associate Professor",
                OfficeRoom = "C1.3.304",
                PhoneNumber = "+7 (7172) 645-704",
                Bio = "Specializes in Relational Database Internals, SQL Optimization, and Transaction Systems.",
                CreatedAt = now
            }
        };

        dbContext.Teachers.AddRange(teachers);
        await dbContext.SaveChangesAsync(cancellationToken);

        var courses = new List<Course>
        {
            new()
            {
                CourseCode = "CS-301",
                Title = "Enterprise Full-Stack Development with .NET & Blazor",
                Description = "Clean Architecture, ASP.NET Core Web API, Entity Framework Core, Blazor Interactive Server, and automated xUnit testing.",
                Credits = 6,
                Department = "Computer Science & Engineering",
                Semester = "Fall 2026",
                TeacherId = teachers[0].Id,
                IsActive = true,
                CreatedAt = now
            },
            new()
            {
                CourseCode = "CS-204",
                Title = "Database Systems & Relational Modeling",
                Description = "Relational algebra, normalization, indexing, ACID transactions, SQLite/PostgreSQL internals, and EF Core query optimization.",
                Credits = 5,
                Department = "Computer Science & Engineering",
                Semester = "Fall 2026",
                TeacherId = teachers[3].Id,
                IsActive = true,
                CreatedAt = now
            },
            new()
            {
                CourseCode = "AI-210",
                Title = "Applied Machine Learning & Data Analytics",
                Description = "Supervised and unsupervised learning, feature engineering, predictive modeling, and statistical performance evaluation.",
                Credits = 5,
                Department = "Artificial Intelligence & Big Data",
                Semester = "Fall 2026",
                TeacherId = teachers[1].Id,
                IsActive = true,
                CreatedAt = now
            },
            new()
            {
                CourseCode = "SEC-305",
                Title = "Application Security & Cryptography",
                Description = "Authentication protocols, PBKDF2/Argon2 password hashing, OWASP Top 10 mitigation, RBAC, and TLS architecture.",
                Credits = 4,
                Department = "Information Security",
                Semester = "Fall 2026",
                TeacherId = teachers[2].Id,
                IsActive = true,
                CreatedAt = now
            },
            new()
            {
                CourseCode = "SE-312",
                Title = "Software Quality Assurance & Automated Testing",
                Description = "Unit testing, integration testing, TDD, continuous integration pipelines, and software reliability metrics.",
                Credits = 4,
                Department = "Computer Science & Engineering",
                Semester = "Fall 2026",
                TeacherId = teachers[0].Id,
                IsActive = true,
                CreatedAt = now
            }
        };

        dbContext.Courses.AddRange(courses);
        await dbContext.SaveChangesAsync(cancellationToken);

        // Schedule entries across Monday - Friday
        var schedules = new List<ScheduleEntry>
        {
            new() { CourseId = courses[0].Id, DayOfWeek = DayOfWeek.Monday, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(10, 30), Classroom = "C1.3.320", LessonType = "Lecture" },
            new() { CourseId = courses[0].Id, DayOfWeek = DayOfWeek.Wednesday, StartTime = new TimeOnly(11, 0), EndTime = new TimeOnly(12, 30), Classroom = "Lab C1.2.240", LessonType = "Practice" },
            new() { CourseId = courses[1].Id, DayOfWeek = DayOfWeek.Tuesday, StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(11, 30), Classroom = "C1.3.305", LessonType = "Lecture" },
            new() { CourseId = courses[1].Id, DayOfWeek = DayOfWeek.Thursday, StartTime = new TimeOnly(14, 0), EndTime = new TimeOnly(15, 30), Classroom = "Lab C1.2.212", LessonType = "Lab" },
            new() { CourseId = courses[2].Id, DayOfWeek = DayOfWeek.Monday, StartTime = new TimeOnly(13, 0), EndTime = new TimeOnly(14, 30), Classroom = "C1.2.210", LessonType = "Lecture" },
            new() { CourseId = courses[2].Id, DayOfWeek = DayOfWeek.Friday, StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(11, 30), Classroom = "Lab C1.2.218", LessonType = "Lab" },
            new() { CourseId = courses[3].Id, DayOfWeek = DayOfWeek.Wednesday, StartTime = new TimeOnly(14, 0), EndTime = new TimeOnly(15, 30), Classroom = "C1.1.115", LessonType = "Lecture" },
            new() { CourseId = courses[4].Id, DayOfWeek = DayOfWeek.Thursday, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(10, 30), Classroom = "C1.3.318", LessonType = "Seminar" }
        };

        dbContext.ScheduleEntries.AddRange(schedules);
        await dbContext.SaveChangesAsync(cancellationToken);

        var students = await dbContext.Students.OrderBy(s => s.Id).ToListAsync(cancellationToken);
        if (students.Count == 0)
        {
            return;
        }

        var aidar = students[0];
        var alina = students.Count > 1 ? students[1] : students[0];
        var timur = students.Count > 2 ? students[2] : students[0];

        var enrollments = new List<Enrollment>
        {
            new() { StudentId = aidar.Id, CourseId = courses[0].Id, Semester = "Fall 2026", Status = EnrollmentStatus.Enrolled, EnrolledAt = now.AddDays(-45) },
            new() { StudentId = aidar.Id, CourseId = courses[1].Id, Semester = "Fall 2026", Status = EnrollmentStatus.Enrolled, EnrolledAt = now.AddDays(-45) },
            new() { StudentId = aidar.Id, CourseId = courses[3].Id, Semester = "Fall 2026", Status = EnrollmentStatus.Enrolled, EnrolledAt = now.AddDays(-45) },
            new() { StudentId = aidar.Id, CourseId = courses[4].Id, Semester = "Fall 2026", Status = EnrollmentStatus.Enrolled, EnrolledAt = now.AddDays(-45) }
        };

        if (alina.Id != aidar.Id)
        {
            enrollments.AddRange(
            [
                new() { StudentId = alina.Id, CourseId = courses[0].Id, Semester = "Fall 2026", Status = EnrollmentStatus.Enrolled, EnrolledAt = now.AddDays(-44) },
                new() { StudentId = alina.Id, CourseId = courses[1].Id, Semester = "Fall 2026", Status = EnrollmentStatus.Enrolled, EnrolledAt = now.AddDays(-44) },
                new() { StudentId = alina.Id, CourseId = courses[2].Id, Semester = "Fall 2026", Status = EnrollmentStatus.Enrolled, EnrolledAt = now.AddDays(-44) }
            ]);
        }

        if (timur.Id != aidar.Id && timur.Id != alina.Id)
        {
            enrollments.AddRange(
            [
                new() { StudentId = timur.Id, CourseId = courses[3].Id, Semester = "Fall 2026", Status = EnrollmentStatus.Enrolled, EnrolledAt = now.AddDays(-42) },
                new() { StudentId = timur.Id, CourseId = courses[1].Id, Semester = "Fall 2026", Status = EnrollmentStatus.Enrolled, EnrolledAt = now.AddDays(-42) }
            ]);
        }

        dbContext.Enrollments.AddRange(enrollments);
        await dbContext.SaveChangesAsync(cancellationToken);

        var grades = new List<Grade>
        {
            new() { StudentId = aidar.Id, CourseId = courses[0].Id, AssessmentTitle = "Architecture & Domain Modeling Lab", AssessmentType = "Assignment", Score = 96.0m, Weight = 0.4m, Feedback = "Clean separation of Core and Infrastructure layers.", GradedAt = now.AddDays(-20) },
            new() { StudentId = aidar.Id, CourseId = courses[0].Id, AssessmentTitle = "Midterm Practical Exam", AssessmentType = "Midterm", Score = 93.0m, Weight = 0.6m, Feedback = "Excellent Blazor Server state management.", GradedAt = now.AddDays(-7) },
            new() { StudentId = aidar.Id, CourseId = courses[1].Id, AssessmentTitle = "Normalized Schema & Indexing Quiz", AssessmentType = "Quiz", Score = 88.0m, Weight = 0.5m, Feedback = "Strong composite index selection.", GradedAt = now.AddDays(-18) },
            new() { StudentId = aidar.Id, CourseId = courses[1].Id, AssessmentTitle = "Midterm SQL & EF Core Exam", AssessmentType = "Midterm", Score = 85.0m, Weight = 0.5m, Feedback = "Well-structured migrations and queries.", GradedAt = now.AddDays(-6) },
            new() { StudentId = aidar.Id, CourseId = courses[3].Id, AssessmentTitle = "Authentication & RBAC Audit", AssessmentType = "Assignment", Score = 92.0m, Weight = 1.0m, Feedback = "Proper PBKDF2 hashing and ownership checks.", GradedAt = now.AddDays(-10) },
            new() { StudentId = aidar.Id, CourseId = courses[4].Id, AssessmentTitle = "xUnit Test Suite Coverage Lab", AssessmentType = "Assignment", Score = 95.0m, Weight = 1.0m, Feedback = "Comprehensive positive and negative test cases.", GradedAt = now.AddDays(-4) }
        };

        if (alina.Id != aidar.Id)
        {
            grades.AddRange(
            [
                new() { StudentId = alina.Id, CourseId = courses[2].Id, AssessmentTitle = "Exploratory Data Analysis Project", AssessmentType = "Assignment", Score = 97.0m, Weight = 0.5m, Feedback = "Outstanding statistical rigor.", GradedAt = now.AddDays(-14) },
                new() { StudentId = alina.Id, CourseId = courses[2].Id, AssessmentTitle = "Machine Learning Midterm", AssessmentType = "Midterm", Score = 91.0m, Weight = 0.5m, Feedback = "Great model evaluation metrics.", GradedAt = now.AddDays(-5) },
                new() { StudentId = alina.Id, CourseId = courses[0].Id, AssessmentTitle = "Midterm Practical Exam", AssessmentType = "Midterm", Score = 89.0m, Weight = 1.0m, Feedback = "Responsive UI components.", GradedAt = now.AddDays(-7) }
            ]);
        }

        dbContext.Grades.AddRange(grades);
        await dbContext.SaveChangesAsync(cancellationToken);

        var today = DateOnly.FromDateTime(now);
        var attendance = new List<AttendanceRecord>
        {
            new() { StudentId = aidar.Id, CourseId = courses[0].Id, Date = today.AddDays(-14), Status = AttendanceStatus.Present, Topic = "Clean Architecture in .NET 10", RecordedAt = now.AddDays(-14) },
            new() { StudentId = aidar.Id, CourseId = courses[0].Id, Date = today.AddDays(-10), Status = AttendanceStatus.Present, Topic = "Blazor Interactive Server Components", RecordedAt = now.AddDays(-10) },
            new() { StudentId = aidar.Id, CourseId = courses[0].Id, Date = today.AddDays(-7), Status = AttendanceStatus.Present, Topic = "Role-Based Access Control", RecordedAt = now.AddDays(-7) },
            new() { StudentId = aidar.Id, CourseId = courses[1].Id, Date = today.AddDays(-12), Status = AttendanceStatus.Present, Topic = "Entity Framework Core Migrations", RecordedAt = now.AddDays(-12) },
            new() { StudentId = aidar.Id, CourseId = courses[1].Id, Date = today.AddDays(-5), Status = AttendanceStatus.Late, Topic = "Query Performance & Indexing", Notes = "Arrived 10 minutes late due to transit delay", RecordedAt = now.AddDays(-5) },
            new() { StudentId = aidar.Id, CourseId = courses[3].Id, Date = today.AddDays(-9), Status = AttendanceStatus.Present, Topic = "Password Hashing & OWASP Security", RecordedAt = now.AddDays(-9) },
            new() { StudentId = aidar.Id, CourseId = courses[4].Id, Date = today.AddDays(-3), Status = AttendanceStatus.Present, Topic = "Automated Unit & Integration Testing", RecordedAt = now.AddDays(-3) }
        };

        if (alina.Id != aidar.Id)
        {
            attendance.AddRange(
            [
                new() { StudentId = alina.Id, CourseId = courses[2].Id, Date = today.AddDays(-11), Status = AttendanceStatus.Present, Topic = "Regression & Classification Pipelines", RecordedAt = now.AddDays(-11) },
                new() { StudentId = alina.Id, CourseId = courses[2].Id, Date = today.AddDays(-4), Status = AttendanceStatus.Present, Topic = "Cross-Validation & Regularization", RecordedAt = now.AddDays(-4) },
                new() { StudentId = alina.Id, CourseId = courses[0].Id, Date = today.AddDays(-7), Status = AttendanceStatus.Absent, Topic = "Role-Based Access Control", Notes = "Excused for academic conference", RecordedAt = now.AddDays(-7) }
            ]);
        }

        dbContext.AttendanceRecords.AddRange(attendance);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger?.LogInformation("Seeded Stage 2 Academic Management data (Teachers, Courses, Enrollments, Grades, Schedule, Attendance).");
    }
}
