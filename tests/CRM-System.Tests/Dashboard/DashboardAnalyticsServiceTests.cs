using CRM_System.Core.Academic.DTOs;
using CRM_System.Core.Academic.Models;
using CRM_System.Core.Academic.Services;
using CRM_System.Core.Common;
using CRM_System.Core.Dashboard.DTOs;
using CRM_System.Core.Dashboard.Services;
using CRM_System.Core.Students.DTOs;
using CRM_System.Core.Students.Services;
using CRM_System.Core.Tasks.DTOs;
using CRM_System.Core.Tasks.Models;
using CRM_System.Core.Tasks.Services;
using CRM_System.Infrastructure.Academic;
using CRM_System.Infrastructure.Dashboard;
using CRM_System.Infrastructure.Data;
using CRM_System.Infrastructure.Students;
using CRM_System.Infrastructure.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CRM_System.Tests.Dashboard;

public class DashboardAnalyticsServiceTests
{
    private static (
        CrmDbContext Db,
        StudentService StudentService,
        AcademicService AcademicService,
        TaskNotificationService TaskService,
        DashboardAnalyticsService AnalyticsService) CreateSut()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"CrmDashboardAnalyticsTests_{Guid.NewGuid():N}")
            .Options;

        var db = new CrmDbContext(options);
        var hasher = new PasswordHasherAdapter();
        var userRepo = new UserRepository(db);
        var studentRepo = new StudentRepository(db);
        var academicRepo = new AcademicRepository(db);
        var taskRepo = new TaskNotificationRepository(db);
        var dashboardRepo = new DashboardAnalyticsRepository(db);
        var gpaCalculator = new ConfigurableGpaCalculator();

        var studentService = new StudentService(studentRepo, userRepo, hasher);
        var academicService = new AcademicService(academicRepo, studentRepo, gpaCalculator);
        var taskService = new TaskNotificationService(taskRepo, studentRepo, academicRepo);
        var analyticsService = new DashboardAnalyticsService(dashboardRepo, gpaCalculator);

        return (db, studentService, academicService, taskService, analyticsService);
    }

    [Fact]
    public async Task GetDashboardOverview_AdminRole_ComputesAccurateKpisDeadlinesNotificationsAndRecentActivity()
    {
        var (_, studentService, academicService, taskService, analyticsService) = CreateSut();
        var admin = AccessContext.ForAdmin(1);
        var now = new DateTime(2026, 11, 14, 12, 0, 0, DateTimeKind.Utc);

        var student1 = (await studentService.CreateStudentAsync(new CreateStudentRequestDto
        {
            FullName = "Aidar Nurlanov",
            Email = "aidar@student.crm.edu",
            StudyProgram = "Computer Science",
            StudyYear = 3,
            CreateLoginAccount = true
        }, admin)).Data!;

        var student2 = (await studentService.CreateStudentAsync(new CreateStudentRequestDto
        {
            FullName = "Dana Kassymova",
            Email = "dana@student.crm.edu",
            StudyProgram = "Software Engineering",
            StudyYear = 2,
            CreateLoginAccount = true
        }, admin)).Data!;

        var teacher = (await academicService.CreateTeacherAsync(new SaveTeacherRequestDto
        {
            FullName = "Dr. Timur Akhmetov",
            Email = "timur.akhmetov@crm.edu",
            Department = "Computer Science",
            AcademicTitle = "Professor"
        }, admin)).Data!;

        var course = (await academicService.CreateCourseAsync(new SaveCourseRequestDto
        {
            CourseCode = "CS-301",
            Title = "Enterprise .NET Architecture",
            Description = "Full-stack C# and Blazor.",
            Credits = 6,
            Department = "Computer Science",
            Semester = "Fall 2026",
            TeacherId = teacher.Id
        }, admin)).Data!;

        await academicService.EnrollStudentAsync(new CreateEnrollmentRequestDto
        {
            StudentId = student1.Id,
            CourseId = course.Id,
            Semester = "Fall 2026"
        }, admin);

        await academicService.EnrollStudentAsync(new CreateEnrollmentRequestDto
        {
            StudentId = student2.Id,
            CourseId = course.Id,
            Semester = "Fall 2026"
        }, admin);

        await academicService.AddGradeAsync(new SaveGradeRequestDto
        {
            StudentId = student1.Id,
            CourseId = course.Id,
            AssessmentTitle = "Midterm Exam",
            AssessmentType = "Exam",
            Score = 95m,
            Weight = 1.0m
        }, admin);

        await academicService.AddGradeAsync(new SaveGradeRequestDto
        {
            StudentId = student2.Id,
            CourseId = course.Id,
            AssessmentTitle = "Midterm Exam",
            AssessmentType = "Exam",
            Score = 75m,
            Weight = 1.0m
        }, admin);

        await academicService.RecordAttendanceAsync(new SaveAttendanceRequestDto
        {
            StudentId = student1.Id,
            CourseId = course.Id,
            Date = new DateOnly(2026, 11, 10),
            Status = AttendanceStatus.Present,
            Topic = "Clean Architecture"
        }, admin);

        await academicService.RecordAttendanceAsync(new SaveAttendanceRequestDto
        {
            StudentId = student2.Id,
            CourseId = course.Id,
            Date = new DateOnly(2026, 11, 10),
            Status = AttendanceStatus.Late,
            Topic = "Clean Architecture"
        }, admin);

        await taskService.CreateTaskAsync(new SaveStudentTaskRequestDto
        {
            StudentId = student1.Id,
            CourseId = course.Id,
            Title = "Stage 4 Analytics Implementation",
            Description = "Implement dashboard KPIs and charts.",
            DueDate = now.AddHours(24),
            Priority = TaskPriority.High,
            Status = StudentTaskStatus.InProgress
        }, admin, now);

        await taskService.CreateNotificationAsync(new CreateNotificationRequestDto
        {
            StudentId = student1.Id,
            UserId = student1.UserId,
            RelatedCourseId = course.Id,
            Title = "Midterm Grade Published",
            Message = "Your CS-301 Midterm grade is available.",
            Type = NotificationType.GradePosted
        }, admin);

        var overviewRes = await analyticsService.GetDashboardOverviewAsync(null, admin, now);

        Assert.True(overviewRes.Succeeded);
        var overview = overviewRes.Data!;

        Assert.Equal(2, overview.Kpis.TotalStudents);
        Assert.Equal(1, overview.Kpis.TotalCourses);
        Assert.Equal(1, overview.Kpis.TotalTeachers);
        Assert.Equal(2, overview.Kpis.TotalEnrollments);
        Assert.Equal(3.00m, overview.Kpis.AverageGpa); // Student 1: 4.0 (A), Student 2: 2.0 (C) => Avg 3.00
        Assert.Equal(85.0m, overview.Kpis.OverallScorePercentage);
        Assert.Equal(75.0m, overview.Kpis.OverallAttendancePercentage); // 1 Present (1.0) + 1 Late (0.5) / 2 = 75.0%
        Assert.Equal(1, overview.Kpis.TotalTasks);
        Assert.Equal(1, overview.Kpis.ActiveTasks);
        Assert.Equal(1, overview.Kpis.DueSoonTasks);
        Assert.Equal(3, overview.Kpis.UnreadNotificationsCount); // TaskAssigned + DeadlineReminder + GradePosted
        Assert.Single(overview.UpcomingDeadlines);
        Assert.Equal(3, overview.RecentNotifications.Count);
        Assert.NotEmpty(overview.RecentActivity);
    }

    [Fact]
    public async Task GetAcademicAnalytics_ComputesAverageGpa_GpaAndGradeDistributions_CourseAndStudentRankings_AndHighestLowestPerformers()
    {
        var (_, studentService, academicService, _, analyticsService) = CreateSut();
        var admin = AccessContext.ForAdmin(1);

        var sHigh = (await studentService.CreateStudentAsync(new CreateStudentRequestDto
        {
            FullName = "Aruzhanbekova",
            Email = "aruzhan@student.crm.edu",
            CreateLoginAccount = true
        }, admin)).Data!;

        var sMid = (await studentService.CreateStudentAsync(new CreateStudentRequestDto
        {
            FullName = "Bekzat Alimov",
            Email = "bekzat@student.crm.edu",
            CreateLoginAccount = true
        }, admin)).Data!;

        var sLow = (await studentService.CreateStudentAsync(new CreateStudentRequestDto
        {
            FullName = "Dias Serikov",
            Email = "dias@student.crm.edu",
            CreateLoginAccount = true
        }, admin)).Data!;

        var c1 = (await academicService.CreateCourseAsync(new SaveCourseRequestDto
        {
            CourseCode = "CS-101",
            Title = "Algorithms",
            Description = "Core algorithms.",
            Credits = 5,
            Department = "CS",
            Semester = "Fall 2026"
        }, admin)).Data!;

        var c2 = (await academicService.CreateCourseAsync(new SaveCourseRequestDto
        {
            CourseCode = "CS-202",
            Title = "Databases",
            Description = "Relational systems.",
            Credits = 5,
            Department = "CS",
            Semester = "Fall 2026"
        }, admin)).Data!;

        // sHigh -> 96% in CS-101 (A, 4.0)
        await academicService.AddGradeAsync(new SaveGradeRequestDto
        {
            StudentId = sHigh.Id,
            CourseId = c1.Id,
            AssessmentTitle = "Final",
            AssessmentType = "Exam",
            Score = 96m,
            Weight = 1.0m
        }, admin);

        // sMid -> 84% in CS-101 (B, 3.0)
        await academicService.AddGradeAsync(new SaveGradeRequestDto
        {
            StudentId = sMid.Id,
            CourseId = c1.Id,
            AssessmentTitle = "Final",
            AssessmentType = "Exam",
            Score = 84m,
            Weight = 1.0m
        }, admin);

        // sLow -> 52% in CS-202 (F, 0.0)
        await academicService.AddGradeAsync(new SaveGradeRequestDto
        {
            StudentId = sLow.Id,
            CourseId = c2.Id,
            AssessmentTitle = "Final",
            AssessmentType = "Exam",
            Score = 52m,
            Weight = 1.0m
        }, admin);

        var res = await analyticsService.GetAcademicAnalyticsAsync(null, admin);

        Assert.True(res.Succeeded);
        var data = res.Data!;

        // GPAs: 4.00, 3.00, 0.00 -> Average = 2.33, Median = 3.00, Highest = 4.00, Lowest = 0.00
        Assert.Equal(2.33m, data.AverageGpa);
        Assert.Equal(3.00m, data.MedianGpa);
        Assert.Equal(4.00m, data.HighestGpa);
        Assert.Equal(0.00m, data.LowestGpa);
        Assert.Equal(3, data.TotalGradedStudents);
        Assert.Equal(3, data.TotalAssessmentsCount);

        Assert.NotNull(data.HighestPerformer);
        Assert.Equal(sHigh.Id, data.HighestPerformer!.StudentId);
        Assert.Equal("Dean's List", data.HighestPerformer.StandingLabel);

        Assert.NotNull(data.LowestPerformer);
        Assert.Equal(sLow.Id, data.LowestPerformer!.StudentId);
        Assert.Equal("At Risk", data.LowestPerformer.StandingLabel);

        Assert.NotNull(data.TopPerformingCourse);
        Assert.Equal("CS-101", data.TopPerformingCourse!.CourseCode);
        Assert.Equal(90.0m, data.TopPerformingCourse.AverageScorePercentage);

        Assert.NotNull(data.LowestPerformingCourse);
        Assert.Equal("CS-202", data.LowestPerformingCourse!.CourseCode);
        Assert.Equal(52.0m, data.LowestPerformingCourse.AverageScorePercentage);

        // Grade distribution: 1 A, 1 B, 1 F
        Assert.Equal(1, data.GradeDistribution.Single(g => g.LetterGrade == "A").AssessmentCount);
        Assert.Equal(1, data.GradeDistribution.Single(g => g.LetterGrade == "B").AssessmentCount);
        Assert.Equal(1, data.GradeDistribution.Single(g => g.LetterGrade == "F").AssessmentCount);

        // GPA distribution: 1 in 3.50-4.00, 1 in 3.00-3.49, 1 in 0.00-0.99
        Assert.Equal(1, data.GpaDistribution.Single(b => b.MinGpa == 3.50m).StudentCount);
        Assert.Equal(1, data.GpaDistribution.Single(b => b.MinGpa == 3.00m).StudentCount);
        Assert.Equal(1, data.GpaDistribution.Single(b => b.MinGpa == 0.00m).StudentCount);
    }

    [Fact]
    public async Task GetAttendanceAnalytics_ComputesOverallRate_CourseAndStudentBreakdowns_StatusDistribution_AndTrends()
    {
        var (_, studentService, academicService, _, analyticsService) = CreateSut();
        var admin = AccessContext.ForAdmin(1);

        var s1 = (await studentService.CreateStudentAsync(new CreateStudentRequestDto
        {
            FullName = "Student One",
            Email = "s1@student.crm.edu",
            CreateLoginAccount = true
        }, admin)).Data!;

        var s2 = (await studentService.CreateStudentAsync(new CreateStudentRequestDto
        {
            FullName = "Student Two",
            Email = "s2@student.crm.edu",
            CreateLoginAccount = true
        }, admin)).Data!;

        var course = (await academicService.CreateCourseAsync(new SaveCourseRequestDto
        {
            CourseCode = "SE-210",
            Title = "Software Testing",
            Description = "Automated testing with xUnit.",
            Credits = 5,
            Department = "SE",
            Semester = "Fall 2026"
        }, admin)).Data!;

        // s1: 2 Present -> 100%
        await academicService.RecordAttendanceAsync(new SaveAttendanceRequestDto
        {
            StudentId = s1.Id,
            CourseId = course.Id,
            Date = new DateOnly(2026, 11, 1),
            Status = AttendanceStatus.Present
        }, admin);

        await academicService.RecordAttendanceAsync(new SaveAttendanceRequestDto
        {
            StudentId = s1.Id,
            CourseId = course.Id,
            Date = new DateOnly(2026, 11, 8),
            Status = AttendanceStatus.Present
        }, admin);

        // s2: 1 Late, 1 Absent -> 25% (At Risk < 75%)
        await academicService.RecordAttendanceAsync(new SaveAttendanceRequestDto
        {
            StudentId = s2.Id,
            CourseId = course.Id,
            Date = new DateOnly(2026, 11, 1),
            Status = AttendanceStatus.Late
        }, admin);

        await academicService.RecordAttendanceAsync(new SaveAttendanceRequestDto
        {
            StudentId = s2.Id,
            CourseId = course.Id,
            Date = new DateOnly(2026, 11, 8),
            Status = AttendanceStatus.Absent
        }, admin);

        var res = await analyticsService.GetAttendanceAnalyticsAsync(null, admin);

        Assert.True(res.Succeeded);
        var att = res.Data!;

        Assert.Equal(4, att.TotalSessions);
        Assert.Equal(2, att.PresentCount);
        Assert.Equal(1, att.LateCount);
        Assert.Equal(1, att.AbsentCount);
        Assert.Equal(62.5m, att.OverallAttendancePercentage); // (2 + 0.5) / 4 = 62.5%
        Assert.Equal(50.0m, att.StrictPresentPercentage);
        Assert.Equal(1, att.AtRiskStudentsCount);
        Assert.Equal(2, att.AttendanceTrends.Count);

        var s2Stat = att.StudentStatistics.Single(s => s.StudentId == s2.Id);
        Assert.True(s2Stat.IsAtRisk);
        Assert.Equal(25.0m, s2Stat.AttendanceRatePercentage);
    }

    [Fact]
    public async Task GetTaskProductivityAnalytics_ComputesTaskCounts_PriorityStatusCourseBreakdowns_AndCompletionRates()
    {
        var (_, studentService, academicService, taskService, analyticsService) = CreateSut();
        var admin = AccessContext.ForAdmin(1);
        var now = new DateTime(2026, 11, 14, 12, 0, 0, DateTimeKind.Utc);

        var student = (await studentService.CreateStudentAsync(new CreateStudentRequestDto
        {
            FullName = "Aidar Nurlanov",
            Email = "aidar@student.crm.edu",
            CreateLoginAccount = true
        }, admin)).Data!;

        var course = (await academicService.CreateCourseAsync(new SaveCourseRequestDto
        {
            CourseCode = "CS-301",
            Title = "Enterprise .NET",
            Description = "Blazor and Web API.",
            Credits = 5,
            Department = "CS",
            Semester = "Fall 2026"
        }, admin)).Data!;

        // 1. Completed High priority task
        await taskService.CreateTaskAsync(new SaveStudentTaskRequestDto
        {
            StudentId = student.Id,
            CourseId = course.Id,
            Title = "Completed Task",
            Description = "Finished on time.",
            DueDate = now.AddDays(1),
            Priority = TaskPriority.High,
            Status = StudentTaskStatus.Completed
        }, admin, now);

        // 2. Overdue High priority task (past DueDate, still Pending)
        await taskService.CreateTaskAsync(new SaveStudentTaskRequestDto
        {
            StudentId = student.Id,
            CourseId = course.Id,
            Title = "Overdue Task",
            Description = "Missed deadline.",
            DueDate = now.AddDays(-2),
            Priority = TaskPriority.High,
            Status = StudentTaskStatus.Pending
        }, admin, now);

        // 3. Due-soon Medium priority task (within 24h)
        await taskService.CreateTaskAsync(new SaveStudentTaskRequestDto
        {
            StudentId = student.Id,
            CourseId = course.Id,
            Title = "Due Soon Task",
            Description = "Due tomorrow.",
            DueDate = now.AddHours(20),
            Priority = TaskPriority.Medium,
            Status = StudentTaskStatus.InProgress
        }, admin, now);

        // 4. Future Low priority general task
        await taskService.CreateTaskAsync(new SaveStudentTaskRequestDto
        {
            StudentId = student.Id,
            CourseId = null,
            Title = "Future Personal Task",
            Description = "Next week.",
            DueDate = now.AddDays(7),
            Priority = TaskPriority.Low,
            Status = StudentTaskStatus.Pending
        }, admin, now);

        var res = await analyticsService.GetTaskProductivityAnalyticsAsync(null, admin, now);

        Assert.True(res.Succeeded);
        var tasks = res.Data!;

        Assert.Equal(4, tasks.TotalTasks);
        Assert.Equal(1, tasks.CompletedTasks);
        Assert.Equal(1, tasks.InProgressTasks);
        Assert.Equal(1, tasks.PendingTasks);
        Assert.Equal(1, tasks.OverdueTasks);
        Assert.Equal(3, tasks.ActiveTasks);
        Assert.Equal(1, tasks.DueSoonTasks);
        Assert.Equal(25.0m, tasks.CompletionRatePercentage);
        Assert.Equal(100.0m, tasks.OnTimeCompletionRatePercentage);

        var highPrio = tasks.PriorityBreakdown.Single(p => p.Priority == TaskPriority.High);
        Assert.Equal(2, highPrio.TotalCount);
        Assert.Equal(1, highPrio.CompletedCount);
        Assert.Equal(1, highPrio.OverdueCount);
        Assert.Equal(50.0m, highPrio.CompletionRatePercentage);

        Assert.Equal(2, tasks.UpcomingDeadlines.Count);
        Assert.Single(tasks.OverdueTaskItems);
    }

    [Fact]
    public async Task DashboardFiltering_ByDateRangeCourseStudentAndSemester_FiltersDisplayedDataAccurately()
    {
        var (_, studentService, academicService, taskService, analyticsService) = CreateSut();
        var admin = AccessContext.ForAdmin(1);
        var now = new DateTime(2026, 11, 14, 12, 0, 0, DateTimeKind.Utc);

        var s1 = (await studentService.CreateStudentAsync(new CreateStudentRequestDto
        {
            FullName = "Student Alpha",
            Email = "alpha@student.crm.edu",
            CreateLoginAccount = true
        }, admin)).Data!;

        var s2 = (await studentService.CreateStudentAsync(new CreateStudentRequestDto
        {
            FullName = "Student Beta",
            Email = "beta@student.crm.edu",
            CreateLoginAccount = true
        }, admin)).Data!;

        var fallCourse = (await academicService.CreateCourseAsync(new SaveCourseRequestDto
        {
            CourseCode = "CS-FALL",
            Title = "Fall Systems",
            Description = "Fall semester course.",
            Credits = 5,
            Department = "CS",
            Semester = "Fall 2026"
        }, admin)).Data!;

        var springCourse = (await academicService.CreateCourseAsync(new SaveCourseRequestDto
        {
            CourseCode = "CS-SPR",
            Title = "Spring AI",
            Description = "Spring semester course.",
            Credits = 5,
            Department = "CS",
            Semester = "Spring 2026"
        }, admin)).Data!;

        await academicService.RecordAttendanceAsync(new SaveAttendanceRequestDto
        {
            StudentId = s1.Id,
            CourseId = fallCourse.Id,
            Date = new DateOnly(2026, 10, 5),
            Status = AttendanceStatus.Present
        }, admin);

        await academicService.RecordAttendanceAsync(new SaveAttendanceRequestDto
        {
            StudentId = s2.Id,
            CourseId = springCourse.Id,
            Date = new DateOnly(2026, 11, 12),
            Status = AttendanceStatus.Absent
        }, admin);

        await taskService.CreateTaskAsync(new SaveStudentTaskRequestDto
        {
            StudentId = s1.Id,
            CourseId = fallCourse.Id,
            Title = "October Task",
            Description = "Due in October.",
            DueDate = new DateTime(2026, 10, 10, 12, 0, 0, DateTimeKind.Utc),
            Priority = TaskPriority.Medium,
            Status = StudentTaskStatus.Completed
        }, admin, now);

        await taskService.CreateTaskAsync(new SaveStudentTaskRequestDto
        {
            StudentId = s2.Id,
            CourseId = springCourse.Id,
            Title = "November Task",
            Description = "Due in November.",
            DueDate = new DateTime(2026, 11, 20, 12, 0, 0, DateTimeKind.Utc),
            Priority = TaskPriority.High,
            Status = StudentTaskStatus.Pending
        }, admin, now);

        // 1. Filter by Semester = "Fall 2026"
        var fallRes = await analyticsService.GetDashboardOverviewAsync(new DashboardFilterDto
        {
            Semester = "Fall 2026"
        }, admin, now);

        Assert.True(fallRes.Succeeded);
        Assert.Equal(1, fallRes.Data!.Kpis.TotalCourses);
        Assert.Equal(100.0m, fallRes.Data.Kpis.OverallAttendancePercentage);
        Assert.Equal(1, fallRes.Data.Kpis.TotalTasks);

        // 2. Filter by Date Range (November 2026 only)
        var novRes = await analyticsService.GetDashboardOverviewAsync(new DashboardFilterDto
        {
            DateFrom = new DateOnly(2026, 11, 1),
            DateTo = new DateOnly(2026, 11, 30)
        }, admin, now);

        Assert.True(novRes.Succeeded);
        Assert.Equal(1, novRes.Data!.AttendanceAnalytics.TotalSessions);
        Assert.Equal(0.0m, novRes.Data.AttendanceAnalytics.OverallAttendancePercentage);
        Assert.Equal(1, novRes.Data.TaskAnalytics.TotalTasks);
        Assert.Equal("November Task", novRes.Data.UpcomingDeadlines.Single().Title);

        // 3. Invalid date range (DateFrom > DateTo) returns validation failure
        var invalidDateRes = await analyticsService.GetDashboardOverviewAsync(new DashboardFilterDto
        {
            DateFrom = new DateOnly(2026, 12, 1),
            DateTo = new DateOnly(2026, 11, 1)
        }, admin, now);

        Assert.False(invalidDateRes.Succeeded);
        Assert.Equal(ServiceErrorType.Validation, invalidDateRes.ErrorType);
    }

    [Fact]
    public async Task EmptyDatabaseScenario_AllAnalyticsMethods_ReturnSafeZeroedMetricsWithoutThrowing()
    {
        var (_, _, _, _, analyticsService) = CreateSut();
        var admin = AccessContext.ForAdmin(1);

        var overviewRes = await analyticsService.GetDashboardOverviewAsync(null, admin);
        Assert.True(overviewRes.Succeeded);
        Assert.Equal(0, overviewRes.Data!.Kpis.TotalStudents);
        Assert.Equal(0, overviewRes.Data.Kpis.TotalCourses);
        Assert.Equal(0m, overviewRes.Data.Kpis.AverageGpa);
        Assert.Equal(0m, overviewRes.Data.Kpis.OverallAttendancePercentage);
        Assert.Equal(0, overviewRes.Data.Kpis.TotalTasks);
        Assert.Empty(overviewRes.Data.UpcomingDeadlines);
        Assert.Empty(overviewRes.Data.RecentNotifications);
        Assert.Empty(overviewRes.Data.RecentActivity);

        var academicRes = await analyticsService.GetAcademicAnalyticsAsync(null, admin);
        Assert.True(academicRes.Succeeded);
        Assert.Equal(0m, academicRes.Data!.AverageGpa);
        Assert.Equal(0m, academicRes.Data.MedianGpa);
        Assert.Null(academicRes.Data.HighestPerformer);
        Assert.Null(academicRes.Data.LowestPerformer);

        var attRes = await analyticsService.GetAttendanceAnalyticsAsync(null, admin);
        Assert.True(attRes.Succeeded);
        Assert.Equal(0, attRes.Data!.TotalSessions);
        Assert.Equal(0m, attRes.Data.OverallAttendancePercentage);

        var taskRes = await analyticsService.GetTaskProductivityAnalyticsAsync(null, admin);
        Assert.True(taskRes.Succeeded);
        Assert.Equal(0, taskRes.Data!.TotalTasks);
        Assert.Equal(0m, taskRes.Data.CompletionRatePercentage);
    }

    [Fact]
    public async Task UserSpecificDataIsolation_StudentSeesOnlyOwnData_AndCannotAccessOtherStudentAnalytics()
    {
        var (_, studentService, academicService, taskService, analyticsService) = CreateSut();
        var admin = AccessContext.ForAdmin(1);
        var now = new DateTime(2026, 11, 14, 12, 0, 0, DateTimeKind.Utc);

        var student1 = (await studentService.CreateStudentAsync(new CreateStudentRequestDto
        {
            FullName = "Student One",
            Email = "s1@student.crm.edu",
            CreateLoginAccount = true
        }, admin)).Data!;

        var student2 = (await studentService.CreateStudentAsync(new CreateStudentRequestDto
        {
            FullName = "Student Two",
            Email = "s2@student.crm.edu",
            CreateLoginAccount = true
        }, admin)).Data!;

        var course = (await academicService.CreateCourseAsync(new SaveCourseRequestDto
        {
            CourseCode = "CS-101",
            Title = "Intro to CS",
            Description = "Fundamentals.",
            Credits = 5,
            Department = "CS",
            Semester = "Fall 2026"
        }, admin)).Data!;

        await academicService.AddGradeAsync(new SaveGradeRequestDto
        {
            StudentId = student1.Id,
            CourseId = course.Id,
            AssessmentTitle = "Exam 1",
            AssessmentType = "Exam",
            Score = 92m,
            Weight = 1.0m
        }, admin);

        await academicService.AddGradeAsync(new SaveGradeRequestDto
        {
            StudentId = student2.Id,
            CourseId = course.Id,
            AssessmentTitle = "Exam 1",
            AssessmentType = "Exam",
            Score = 64m,
            Weight = 1.0m
        }, admin);

        await taskService.CreateTaskAsync(new SaveStudentTaskRequestDto
        {
            StudentId = student2.Id,
            CourseId = course.Id,
            Title = "Student 2 Private Task",
            Description = "Should not be visible to Student 1.",
            DueDate = now.AddDays(2),
            Priority = TaskPriority.High,
            Status = StudentTaskStatus.Pending
        }, admin, now);

        var student1Access = AccessContext.ForStudent(student1.UserId!.Value, student1.Id);

        // 1. Student 1 querying with null filter gets strictly their own data
        var ownOverview = await analyticsService.GetDashboardOverviewAsync(null, student1Access, now);
        Assert.True(ownOverview.Succeeded);
        Assert.True(ownOverview.Data!.Kpis.IsStudentScoped);
        Assert.Equal(student1.Id, ownOverview.Data.Kpis.ScopedStudentId);
        Assert.Equal(1, ownOverview.Data.Kpis.TotalStudents);
        Assert.Equal(4.00m, ownOverview.Data.Kpis.AverageGpa);
        Assert.Equal(0, ownOverview.Data.Kpis.TotalTasks); // Student 2's task is not exposed!
        Assert.Single(ownOverview.Data.AcademicAnalytics.StudentPerformances);
        Assert.Equal(student1.Id, ownOverview.Data.AcademicAnalytics.StudentPerformances[0].StudentId);

        // 2. Student 1 attempting to query Student 2's StudentId -> Forbidden
        var forbiddenRes = await analyticsService.GetDashboardOverviewAsync(new DashboardFilterDto
        {
            StudentId = student2.Id
        }, student1Access, now);

        Assert.False(forbiddenRes.Succeeded);
        Assert.Equal(ServiceErrorType.Forbidden, forbiddenRes.ErrorType);

        // 3. Unauthenticated request -> Unauthorized
        var unauthRes = await analyticsService.GetDashboardOverviewAsync(null, AccessContext.Anonymous, now);
        Assert.False(unauthRes.Succeeded);
        Assert.Equal(ServiceErrorType.Unauthorized, unauthRes.ErrorType);
    }

    [Fact]
    public async Task SqliteEndToEndIntegration_SeededDatabase_ProducesCompleteDashboardAndAnalyticsAcrossAllFourStages()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new CrmDbContext(options);
        var hasher = new PasswordHasherAdapter();
        await db.Database.MigrateAsync();
        await DatabaseInitializer.SeedInitialUsersAndStudentsAsync(db, hasher);
        await DatabaseInitializer.SeedAcademicDataAsync(db);
        await DatabaseInitializer.SeedTasksAndNotificationsAsync(db);

        var dashboardRepo = new DashboardAnalyticsRepository(db);
        var gpaCalculator = new ConfigurableGpaCalculator();
        var analyticsService = new DashboardAnalyticsService(dashboardRepo, gpaCalculator);

        var admin = AccessContext.ForAdmin(1);
        var overviewRes = await analyticsService.GetDashboardOverviewAsync(null, admin);

        Assert.True(overviewRes.Succeeded);
        var overview = overviewRes.Data!;

        Assert.Equal(5, overview.Kpis.TotalStudents);
        Assert.Equal(5, overview.Kpis.TotalCourses);
        Assert.Equal(4, overview.Kpis.TotalTeachers);
        Assert.True(overview.Kpis.AverageGpa > 0m);
        Assert.True(overview.Kpis.OverallAttendancePercentage > 0m);
        Assert.Equal(8, overview.Kpis.TotalTasks);
        Assert.NotEmpty(overview.AcademicAnalytics.CoursePerformances);
        Assert.NotEmpty(overview.AcademicAnalytics.StudentPerformances);
        Assert.NotEmpty(overview.AttendanceAnalytics.AttendanceTrends);
        Assert.NotEmpty(overview.TaskAnalytics.PriorityBreakdown);
        Assert.NotEmpty(overview.RecentNotifications);
        Assert.NotEmpty(overview.RecentActivity);
    }
}
