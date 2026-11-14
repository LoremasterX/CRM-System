using CRM_System.Core.Academic.DTOs;
using CRM_System.Core.Academic.Models;
using CRM_System.Core.Academic.Services;
using CRM_System.Core.Common;
using CRM_System.Core.Students.DTOs;
using CRM_System.Core.Students.Services;
using CRM_System.Infrastructure.Academic;
using CRM_System.Infrastructure.Data;
using CRM_System.Infrastructure.Students;
using Microsoft.EntityFrameworkCore;

namespace CRM_System.Tests.Academic;

public class AcademicServiceTests
{
    private static (CrmDbContext Db, StudentService StudentService, AcademicService AcademicService) CreateSut()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"CrmAcademicTests_{Guid.NewGuid():N}")
            .Options;

        var db = new CrmDbContext(options);
        var hasher = new PasswordHasherAdapter();
        var userRepo = new UserRepository(db);
        var studentRepo = new StudentRepository(db);
        var academicRepo = new AcademicRepository(db);
        var gpaCalculator = new ConfigurableGpaCalculator();

        var studentService = new StudentService(studentRepo, userRepo, hasher);
        var academicService = new AcademicService(academicRepo, studentRepo, gpaCalculator);

        return (db, studentService, academicService);
    }

    [Fact]
    public async Task TeacherAndCourseCrud_AdminCreatesAndAssignsTeacher_PreventingDuplicateCodes()
    {
        var (_, _, academic) = CreateSut();
        var admin = AccessContext.ForAdmin(1);

        var teacherResult = await academic.CreateTeacherAsync(new SaveTeacherRequestDto
        {
            FullName = "Dr. Timur Bekturov",
            Email = "t.bekturov@crm.edu",
            Department = "Computer Science & Engineering",
            AcademicTitle = "Professor",
            OfficeRoom = "C1.3.310"
        }, admin);

        Assert.True(teacherResult.Succeeded);
        Assert.NotNull(teacherResult.Data);

        var courseResult = await academic.CreateCourseAsync(new SaveCourseRequestDto
        {
            CourseCode = "CS-301",
            Title = "Enterprise .NET & Blazor Systems",
            Description = "Full-stack C# and Blazor Interactive Server architecture.",
            Credits = 6,
            Department = "Computer Science & Engineering",
            Semester = "Fall 2026",
            TeacherId = teacherResult.Data!.Id
        }, admin);

        Assert.True(courseResult.Succeeded);
        Assert.Equal("CS-301", courseResult.Data!.CourseCode);
        Assert.Equal("Dr. Timur Bekturov", courseResult.Data.TeacherName);

        // Duplicate course code should fail with Conflict
        var duplicateResult = await academic.CreateCourseAsync(new SaveCourseRequestDto
        {
            CourseCode = "cs-301",
            Title = "Duplicate Course",
            Description = "Should fail due to duplicate code.",
            Credits = 4,
            Department = "Computer Science & Engineering",
            Semester = "Fall 2026"
        }, admin);

        Assert.False(duplicateResult.Succeeded);
        Assert.Equal(ServiceErrorType.Conflict, duplicateResult.ErrorType);
    }

    [Fact]
    public async Task StudentEnrollment_AllowsSelfEnrollment_PreventsDuplicate_AllowsReEnrollingDroppedCourse()
    {
        var (_, studentService, academic) = CreateSut();
        var admin = AccessContext.ForAdmin(1);

        var student = (await studentService.CreateStudentAsync(new CreateStudentRequestDto
        {
            FullName = "Aidar Nurlanov",
            Email = "aidar@student.crm.edu",
            CreateLoginAccount = true
        }, admin)).Data!;

        var course = (await academic.CreateCourseAsync(new SaveCourseRequestDto
        {
            CourseCode = "CS-302",
            Title = "Database Engineering",
            Description = "Relational databases and EF Core migrations.",
            Credits = 5,
            Department = "Computer Science",
            Semester = "Fall 2026"
        }, admin)).Data!;

        var studentAccess = AccessContext.ForStudent(student.UserId!.Value, student.Id);

        var enroll1 = await academic.EnrollStudentAsync(new CreateEnrollmentRequestDto
        {
            StudentId = student.Id,
            CourseId = course.Id,
            Semester = "Fall 2026"
        }, studentAccess);

        Assert.True(enroll1.Succeeded);
        Assert.Equal(EnrollmentStatus.Enrolled, enroll1.Data!.Status);

        // Enrolling again while active should return Conflict
        var duplicateEnroll = await academic.EnrollStudentAsync(new CreateEnrollmentRequestDto
        {
            StudentId = student.Id,
            CourseId = course.Id,
            Semester = "Fall 2026"
        }, studentAccess);

        Assert.False(duplicateEnroll.Succeeded);
        Assert.Equal(ServiceErrorType.Conflict, duplicateEnroll.ErrorType);

        // Dropping and re-enrolling should reactivate the enrollment
        var dropResult = await academic.UpdateEnrollmentStatusAsync(enroll1.Data.Id, EnrollmentStatus.Dropped, studentAccess);
        Assert.True(dropResult.Succeeded);
        Assert.Equal(EnrollmentStatus.Dropped, dropResult.Data!.Status);

        var reEnroll = await academic.EnrollStudentAsync(new CreateEnrollmentRequestDto
        {
            StudentId = student.Id,
            CourseId = course.Id,
            Semester = "Fall 2026"
        }, studentAccess);

        Assert.True(reEnroll.Succeeded);
        Assert.Equal(EnrollmentStatus.Enrolled, reEnroll.Data!.Status);
    }

    [Fact]
    public async Task GradesAndGpa_AdminRecordsGrades_StudentReadsOwnGpa_CrossStudentAccessBlocked()
    {
        var (_, studentService, academic) = CreateSut();
        var admin = AccessContext.ForAdmin(1);

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

        var course = (await academic.CreateCourseAsync(new SaveCourseRequestDto
        {
            CourseCode = "SE-304",
            Title = "Software Quality Assurance",
            Description = "Automated testing with xUnit.",
            Credits = 5,
            Department = "Software Engineering",
            Semester = "Fall 2026"
        }, admin)).Data!;

        var addGrade1 = await academic.AddGradeAsync(new SaveGradeRequestDto
        {
            StudentId = student1.Id,
            CourseId = course.Id,
            AssessmentTitle = "Midterm Exam",
            AssessmentType = "Midterm",
            Score = 92m,
            Weight = 0.4m,
            Feedback = "Great unit test coverage."
        }, admin);

        var addGrade2 = await academic.AddGradeAsync(new SaveGradeRequestDto
        {
            StudentId = student1.Id,
            CourseId = course.Id,
            AssessmentTitle = "Final Project",
            AssessmentType = "Final",
            Score = 96m,
            Weight = 0.6m
        }, admin);

        Assert.True(addGrade1.Succeeded);
        Assert.True(addGrade2.Succeeded);

        var student1Access = AccessContext.ForStudent(student1.UserId!.Value, student1.Id);
        var student2Access = AccessContext.ForStudent(student2.UserId!.Value, student2.Id);

        // Student 1 can read own GPA
        var ownGpa = await academic.GetStudentGpaSummaryAsync(student1.Id, student1Access);
        Assert.True(ownGpa.Succeeded);
        Assert.Equal(4.00m, ownGpa.Data!.CumulativeGpa);
        Assert.Equal(94.40m, ownGpa.Data.OverallAveragePercentage);

        // Student 2 cannot read Student 1's GPA or grades
        var crossGpa = await academic.GetStudentGpaSummaryAsync(student1.Id, student2Access);
        Assert.False(crossGpa.Succeeded);
        Assert.Equal(ServiceErrorType.Forbidden, crossGpa.ErrorType);

        // Student cannot record grades
        var unauthorizedAddGrade = await academic.AddGradeAsync(new SaveGradeRequestDto
        {
            StudentId = student1.Id,
            CourseId = course.Id,
            AssessmentTitle = "Self Grade",
            AssessmentType = "Quiz",
            Score = 100m,
            Weight = 1.0m
        }, student1Access);

        Assert.False(unauthorizedAddGrade.Succeeded);
        Assert.Equal(ServiceErrorType.Forbidden, unauthorizedAddGrade.ErrorType);
    }

    [Fact]
    public async Task ScheduleAndUpcomingClasses_OrdersSessionsRelativeToReferenceDayAndTime()
    {
        var (_, studentService, academic) = CreateSut();
        var admin = AccessContext.ForAdmin(1);

        var student = (await studentService.CreateStudentAsync(new CreateStudentRequestDto
        {
            FullName = "Aruzhan Kassymova",
            Email = "aruzhan@student.crm.edu",
            CreateLoginAccount = true
        }, admin)).Data!;

        var course = (await academic.CreateCourseAsync(new SaveCourseRequestDto
        {
            CourseCode = "DS-303",
            Title = "Applied Machine Learning",
            Description = "Statistical learning and predictive analytics.",
            Credits = 5,
            Department = "Data Science",
            Semester = "Fall 2026"
        }, admin)).Data!;

        await academic.EnrollStudentAsync(new CreateEnrollmentRequestDto
        {
            StudentId = student.Id,
            CourseId = course.Id,
            Semester = "Fall 2026"
        }, admin);

        // Add Monday 09:00 and Wednesday 14:00 slots
        await academic.CreateScheduleEntryAsync(new SaveScheduleEntryRequestDto
        {
            CourseId = course.Id,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 30),
            Classroom = "C1.2.210",
            LessonType = "Lecture"
        }, admin);

        await academic.CreateScheduleEntryAsync(new SaveScheduleEntryRequestDto
        {
            CourseId = course.Id,
            DayOfWeek = DayOfWeek.Wednesday,
            StartTime = new TimeOnly(14, 0),
            EndTime = new TimeOnly(15, 30),
            Classroom = "C1.3.108",
            LessonType = "Lab"
        }, admin);

        var studentAccess = AccessContext.ForStudent(student.UserId!.Value, student.Id);

        // Reference time: Tuesday at 10:00 -> Wednesday session should come first, then next Monday
        var tuesdayMorning = new DateTime(2026, 11, 10, 10, 0, 0); // 2026-11-10 is Tuesday
        var upcomingResult = await academic.GetUpcomingClassesAsync(student.Id, tuesdayMorning, 5, studentAccess);

        Assert.True(upcomingResult.Succeeded);
        Assert.Equal(2, upcomingResult.Data!.Count);
        Assert.Equal(DayOfWeek.Wednesday, upcomingResult.Data[0].DayOfWeek);
        Assert.Equal(DayOfWeek.Monday, upcomingResult.Data[1].DayOfWeek);
    }

    [Fact]
    public async Task AttendanceTracking_ComputesAccurateRatesAndEnforcesStudentOwnership()
    {
        var (_, studentService, academic) = CreateSut();
        var admin = AccessContext.ForAdmin(1);

        var student = (await studentService.CreateStudentAsync(new CreateStudentRequestDto
        {
            FullName = "Sanzhar Beketov",
            Email = "sanzhar@student.crm.edu",
            CreateLoginAccount = true
        }, admin)).Data!;

        var course = (await academic.CreateCourseAsync(new SaveCourseRequestDto
        {
            CourseCode = "CS-305",
            Title = "Cloud Native Microservices",
            Description = "Containers and distributed systems.",
            Credits = 5,
            Department = "Computer Science",
            Semester = "Fall 2026"
        }, admin)).Data!;

        // Record 2 Present, 1 Late, 1 Absent => Total 4 sessions
        // AttendanceRatePercentage (Present + Late) = 3 / 4 = 75.00%
        // StrictPresentPercentage (Present only) = 2 / 4 = 50.00%
        await academic.RecordAttendanceAsync(new SaveAttendanceRequestDto
        {
            StudentId = student.Id,
            CourseId = course.Id,
            Date = new DateOnly(2026, 10, 1),
            Status = AttendanceStatus.Present,
            Topic = "Session 1"
        }, admin);

        await academic.RecordAttendanceAsync(new SaveAttendanceRequestDto
        {
            StudentId = student.Id,
            CourseId = course.Id,
            Date = new DateOnly(2026, 10, 8),
            Status = AttendanceStatus.Present,
            Topic = "Session 2"
        }, admin);

        await academic.RecordAttendanceAsync(new SaveAttendanceRequestDto
        {
            StudentId = student.Id,
            CourseId = course.Id,
            Date = new DateOnly(2026, 10, 15),
            Status = AttendanceStatus.Late,
            Topic = "Session 3",
            Notes = "Arrived 10 minutes late"
        }, admin);

        await academic.RecordAttendanceAsync(new SaveAttendanceRequestDto
        {
            StudentId = student.Id,
            CourseId = course.Id,
            Date = new DateOnly(2026, 10, 22),
            Status = AttendanceStatus.Absent,
            Topic = "Session 4"
        }, admin);

        var studentAccess = AccessContext.ForStudent(student.UserId!.Value, student.Id);
        var statsResult = await academic.GetStudentAttendanceStatsAsync(student.Id, studentAccess);

        Assert.True(statsResult.Succeeded);
        Assert.Equal(4, statsResult.Data!.TotalSessions);
        Assert.Equal(2, statsResult.Data.PresentCount);
        Assert.Equal(1, statsResult.Data.LateCount);
        Assert.Equal(1, statsResult.Data.AbsentCount);
        Assert.Equal(75.00m, statsResult.Data.AttendanceRatePercentage);
        Assert.Equal(50.00m, statsResult.Data.StrictPresentPercentage);
        Assert.Single(statsResult.Data.CourseBreakdowns);
    }
}
