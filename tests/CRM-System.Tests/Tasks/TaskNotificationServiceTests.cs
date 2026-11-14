using CRM_System.Core.Academic.DTOs;
using CRM_System.Core.Academic.Services;
using CRM_System.Core.Common;
using CRM_System.Core.Students.DTOs;
using CRM_System.Core.Students.Services;
using CRM_System.Core.Tasks.DTOs;
using CRM_System.Core.Tasks.Models;
using CRM_System.Core.Tasks.Services;
using CRM_System.Infrastructure.Academic;
using CRM_System.Infrastructure.Data;
using CRM_System.Infrastructure.Students;
using CRM_System.Infrastructure.Tasks;
using Microsoft.EntityFrameworkCore;

namespace CRM_System.Tests.Tasks;

public class TaskNotificationServiceTests
{
    private static (CrmDbContext Db, StudentService StudentService, AcademicService AcademicService, TaskNotificationService TaskService) CreateSut()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"CrmTaskNotificationTests_{Guid.NewGuid():N}")
            .Options;

        var db = new CrmDbContext(options);
        var hasher = new PasswordHasherAdapter();
        var userRepo = new UserRepository(db);
        var studentRepo = new StudentRepository(db);
        var academicRepo = new AcademicRepository(db);
        var taskRepo = new TaskNotificationRepository(db);
        var gpaCalculator = new ConfigurableGpaCalculator();

        var studentService = new StudentService(studentRepo, userRepo, hasher);
        var academicService = new AcademicService(academicRepo, studentRepo, gpaCalculator);
        var taskService = new TaskNotificationService(taskRepo, studentRepo, academicRepo);

        return (db, studentService, academicService, taskService);
    }

    [Fact]
    public async Task TaskCrudAndOverdueCalculation_DetectsOverdueAndDueSoonCorrectly()
    {
        var (_, studentService, academic, taskService) = CreateSut();
        var admin = AccessContext.ForAdmin(1);
        var now = new DateTime(2026, 11, 14, 12, 0, 0, DateTimeKind.Utc);

        var student = (await studentService.CreateStudentAsync(new CreateStudentRequestDto
        {
            FullName = "Aidar Nurlanov",
            Email = "aidar@student.crm.edu",
            CreateLoginAccount = true
        }, admin)).Data!;

        var course = (await academic.CreateCourseAsync(new SaveCourseRequestDto
        {
            CourseCode = "CS-301",
            Title = "Enterprise .NET & Blazor",
            Description = "Full-stack architecture.",
            Credits = 6,
            Department = "Computer Science",
            Semester = "Fall 2026"
        }, admin)).Data!;

        var studentAccess = AccessContext.ForStudent(student.UserId!.Value, student.Id);

        // 1. Task due in 24 hours -> IsDueSoon = true, IsOverdue = false
        var dueSoonTask = (await taskService.CreateTaskAsync(new SaveStudentTaskRequestDto
        {
            StudentId = student.Id,
            CourseId = course.Id,
            Title = "Prepare Blazor Component Demo",
            Description = "Build interactive task board.",
            DueDate = now.AddHours(24),
            Priority = TaskPriority.High,
            Status = StudentTaskStatus.InProgress
        }, studentAccess, now)).Data!;

        Assert.True(dueSoonTask.IsDueSoon);
        Assert.False(dueSoonTask.IsOverdue);
        Assert.Equal(StudentTaskStatus.InProgress, dueSoonTask.Status);

        // 2. Task due 2 days ago and still Pending -> automatically evaluated as Overdue
        var overdueTask = (await taskService.CreateTaskAsync(new SaveStudentTaskRequestDto
        {
            StudentId = student.Id,
            CourseId = course.Id,
            Title = "Past Due Lab Submission",
            Description = "Should automatically evaluate as Overdue.",
            DueDate = now.AddDays(-2),
            Priority = TaskPriority.Medium,
            Status = StudentTaskStatus.Pending
        }, studentAccess, now)).Data!;

        Assert.True(overdueTask.IsOverdue);
        Assert.Equal(StudentTaskStatus.Overdue, overdueTask.Status);

        // 3. Completing the overdue task clears its overdue status
        var completedTask = (await taskService.UpdateTaskStatusAsync(overdueTask.Id, StudentTaskStatus.Completed, studentAccess, now)).Data!;
        Assert.False(completedTask.IsOverdue);
        Assert.Equal(StudentTaskStatus.Completed, completedTask.Status);
        Assert.NotNull(completedTask.CompletedAt);
    }

    [Fact]
    public async Task TaskFilteringAndSorting_FiltersByStatusPriorityCourseAndSortsByDeadline()
    {
        var (_, studentService, academic, taskService) = CreateSut();
        var admin = AccessContext.ForAdmin(1);
        var now = new DateTime(2026, 11, 14, 12, 0, 0, DateTimeKind.Utc);

        var student = (await studentService.CreateStudentAsync(new CreateStudentRequestDto
        {
            FullName = "Alina Serikova",
            Email = "alina@student.crm.edu",
            CreateLoginAccount = true
        }, admin)).Data!;

        var course1 = (await academic.CreateCourseAsync(new SaveCourseRequestDto
        {
            CourseCode = "AI-210",
            Title = "Applied Machine Learning",
            Description = "ML pipelines.",
            Credits = 5,
            Department = "AI",
            Semester = "Fall 2026"
        }, admin)).Data!;

        var course2 = (await academic.CreateCourseAsync(new SaveCourseRequestDto
        {
            CourseCode = "CS-204",
            Title = "Database Systems",
            Description = "SQL & indexing.",
            Credits = 5,
            Department = "CS",
            Semester = "Fall 2026"
        }, admin)).Data!;

        var studentAccess = AccessContext.ForStudent(student.UserId!.Value, student.Id);

        await taskService.CreateTaskAsync(new SaveStudentTaskRequestDto
        {
            StudentId = student.Id,
            CourseId = course1.Id,
            Title = "Task B (Later, High Priority)",
            Description = "Train neural network.",
            DueDate = now.AddDays(5),
            Priority = TaskPriority.High,
            Status = StudentTaskStatus.Pending
        }, studentAccess, now);

        await taskService.CreateTaskAsync(new SaveStudentTaskRequestDto
        {
            StudentId = student.Id,
            CourseId = course2.Id,
            Title = "Task A (Earlier, Low Priority)",
            Description = "Write SQL query.",
            DueDate = now.AddDays(1),
            Priority = TaskPriority.Low,
            Status = StudentTaskStatus.InProgress
        }, studentAccess, now);

        // Sort by deadline_asc -> Task A first
        var byDeadlineAsc = (await taskService.GetTasksAsync(new TaskFilterDto { SortBy = "deadline_asc" }, studentAccess, now)).Data!;
        Assert.Equal(2, byDeadlineAsc.Count);
        Assert.StartsWith("Task A", byDeadlineAsc[0].Title);

        // Sort by priority_desc -> Task B first
        var byPriorityDesc = (await taskService.GetTasksAsync(new TaskFilterDto { SortBy = "priority_desc" }, studentAccess, now)).Data!;
        Assert.StartsWith("Task B", byPriorityDesc[0].Title);

        // Filter by Course 1
        var byCourse1 = (await taskService.GetTasksAsync(new TaskFilterDto { CourseId = course1.Id }, studentAccess, now)).Data!;
        Assert.Single(byCourse1);
        Assert.Equal("AI-210", byCourse1[0].CourseCode);

        // Filter by SearchTerm
        var bySearch = (await taskService.GetTasksAsync(new TaskFilterDto { SearchTerm = "neural" }, studentAccess, now)).Data!;
        Assert.Single(bySearch);
    }

    [Fact]
    public async Task NotificationGenerationAndReadManagement_GeneratesIdempotentAlertsAndMarksRead()
    {
        var (_, studentService, academic, taskService) = CreateSut();
        var admin = AccessContext.ForAdmin(1);
        var now = new DateTime(2026, 11, 14, 12, 0, 0, DateTimeKind.Utc);

        var student = (await studentService.CreateStudentAsync(new CreateStudentRequestDto
        {
            FullName = "Timur Akhmetov",
            Email = "timur@student.crm.edu",
            CreateLoginAccount = true
        }, admin)).Data!;

        var course = (await academic.CreateCourseAsync(new SaveCourseRequestDto
        {
            CourseCode = "SEC-305",
            Title = "Application Security",
            Description = "OWASP & cryptography.",
            Credits = 4,
            Department = "Security",
            Semester = "Fall 2026"
        }, admin)).Data!;

        // Admin assigns a task due in 24 hours -> generates BOTH TaskAssigned and DeadlineReminder notifications
        await taskService.CreateTaskAsync(new SaveStudentTaskRequestDto
        {
            StudentId = student.Id,
            CourseId = course.Id,
            Title = "Security Audit Deliverable",
            Description = "Complete penetration test checklist.",
            DueDate = now.AddHours(24),
            Priority = TaskPriority.High,
            Status = StudentTaskStatus.Pending
        }, admin, now);

        var studentAccess = AccessContext.ForStudent(student.UserId!.Value, student.Id);

        // Calling SynchronizeDeadlineNotificationsAsync again must be idempotent (0 duplicate alerts created)
        var syncAgain = await taskService.SynchronizeDeadlineNotificationsAsync(student.Id, studentAccess, now);
        Assert.True(syncAgain.Succeeded);
        Assert.Equal(0, syncAgain.Data);

        var unreadCountBefore = await taskService.GetUnreadCountAsync(student.Id, studentAccess);
        Assert.Equal(2, unreadCountBefore.Data);

        var markAllResult = await taskService.MarkAllNotificationsAsReadAsync(student.Id, studentAccess);
        Assert.True(markAllResult.Succeeded);
        Assert.Equal(2, markAllResult.Data);

        var unreadCountAfter = await taskService.GetUnreadCountAsync(student.Id, studentAccess);
        Assert.Equal(0, unreadCountAfter.Data);
    }

    [Fact]
    public async Task OwnershipAndPermissions_BlocksCrossStudentTaskAndNotificationAccess()
    {
        var (_, studentService, _, taskService) = CreateSut();
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

        var s1Access = AccessContext.ForStudent(student1.UserId!.Value, student1.Id);
        var s2Access = AccessContext.ForStudent(student2.UserId!.Value, student2.Id);

        var s1Task = (await taskService.CreateTaskAsync(new SaveStudentTaskRequestDto
        {
            StudentId = student1.Id,
            Title = "Private Student 1 Task",
            Description = "Confidential study notes.",
            DueDate = now.AddHours(12),
            Priority = TaskPriority.High
        }, s1Access, now)).Data!;

        // Student 2 cannot view, update, or delete Student 1's task
        var crossGet = await taskService.GetTaskByIdAsync(s1Task.Id, s2Access, now);
        var crossStatus = await taskService.UpdateTaskStatusAsync(s1Task.Id, StudentTaskStatus.Completed, s2Access, now);
        var crossDelete = await taskService.DeleteTaskAsync(s1Task.Id, s2Access);
        var crossNotifications = await taskService.GetNotificationsAsync(student1.Id, false, s2Access);

        Assert.False(crossGet.Succeeded);
        Assert.Equal(ServiceErrorType.Forbidden, crossGet.ErrorType);

        Assert.False(crossStatus.Succeeded);
        Assert.Equal(ServiceErrorType.Forbidden, crossStatus.ErrorType);

        Assert.False(crossDelete.Succeeded);
        Assert.Equal(ServiceErrorType.Forbidden, crossDelete.ErrorType);

        Assert.False(crossNotifications.Succeeded);
        Assert.Equal(ServiceErrorType.Forbidden, crossNotifications.ErrorType);
    }

    [Fact]
    public async Task AcademicCalendar_CombinesWeeklyScheduleEntriesAndTaskDeadlines()
    {
        var (_, studentService, academic, taskService) = CreateSut();
        var admin = AccessContext.ForAdmin(1);
        var now = new DateTime(2026, 11, 14, 9, 0, 0, DateTimeKind.Utc);

        var student = (await studentService.CreateStudentAsync(new CreateStudentRequestDto
        {
            FullName = "Aidar Nurlanov",
            Email = "aidar.cal@student.crm.edu",
            CreateLoginAccount = true
        }, admin)).Data!;

        var course = (await academic.CreateCourseAsync(new SaveCourseRequestDto
        {
            CourseCode = "CS-301",
            Title = "Enterprise .NET",
            Description = "Blazor & API.",
            Credits = 6,
            Department = "CS",
            Semester = "Fall 2026"
        }, admin)).Data!;

        await academic.EnrollStudentAsync(new CreateEnrollmentRequestDto
        {
            StudentId = student.Id,
            CourseId = course.Id,
            Semester = "Fall 2026"
        }, admin);

        await academic.CreateScheduleEntryAsync(new SaveScheduleEntryRequestDto
        {
            CourseId = course.Id,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 30),
            Classroom = "C1.3.320",
            LessonType = "Lecture"
        }, admin);

        var studentAccess = AccessContext.ForStudent(student.UserId!.Value, student.Id);

        await taskService.CreateTaskAsync(new SaveStudentTaskRequestDto
        {
            StudentId = student.Id,
            CourseId = course.Id,
            Title = "Milestone 3 Submission",
            Description = "Submit Stage 3 archive.",
            DueDate = new DateTime(2026, 11, 20, 18, 0, 0, DateTimeKind.Utc),
            Priority = TaskPriority.High,
            Status = StudentTaskStatus.Pending
        }, studentAccess, now);

        var calendarRes = await taskService.GetCalendarMonthAsync(student.Id, 2026, 11, studentAccess, now);

        Assert.True(calendarRes.Succeeded);
        Assert.NotNull(calendarRes.Data);
        Assert.Contains(calendarRes.Data!.Events, e => e.EventCategory == "Class" && e.CourseCode == "CS-301");
        Assert.Contains(calendarRes.Data.Events, e => e.EventCategory == "Deadline" && e.Title == "Milestone 3 Submission");
        Assert.Single(calendarRes.Data.UpcomingDeadlines);
    }
}
