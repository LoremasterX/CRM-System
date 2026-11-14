using CRM_System.Core.Common;
using CRM_System.Core.Students.DTOs;
using CRM_System.Core.Students.Models;
using CRM_System.Core.Students.Services;
using CRM_System.Infrastructure.Data;
using CRM_System.Infrastructure.Students;
using Microsoft.EntityFrameworkCore;

namespace CRM_System.Tests.Students;

public class StudentServiceTests
{
    private static (CrmDbContext Db, StudentService Service) CreateSut()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"CrmStudentTests_{Guid.NewGuid():N}")
            .Options;

        var db = new CrmDbContext(options);
        var hasher = new PasswordHasherAdapter();
        var userRepo = new UserRepository(db);
        var studentRepo = new StudentRepository(db);
        var service = new StudentService(studentRepo, userRepo, hasher);

        return (db, service);
    }

    [Fact]
    public async Task CreateStudentAsync_AdminRole_CreatesStudentAndLinkedLoginAccount()
    {
        var (db, service) = CreateSut();
        var adminAccess = AccessContext.ForAdmin(1);

        var result = await service.CreateStudentAsync(new CreateStudentRequestDto
        {
            FullName = "Dias Omarov",
            Email = "dias.omarov@student.crm.edu",
            University = "Astana IT University",
            Faculty = "School of Software Engineering",
            Department = "Computer Science",
            StudyProgram = "B.Sc. Software Engineering",
            StudyYear = 3,
            CreateLoginAccount = true,
            InitialPassword = "Student123!"
        }, adminAccess);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal("Dias Omarov", result.Data!.FullName);
        Assert.NotNull(result.Data.UserId);
        Assert.Equal(1, await db.Users.CountAsync());
        Assert.Equal(1, await db.Students.CountAsync());
    }

    [Fact]
    public async Task CreateStudentAsync_NonAdminRole_ReturnsForbidden()
    {
        var (_, service) = CreateSut();
        var studentAccess = AccessContext.ForStudent(userId: 10, studentId: 5);

        var result = await service.CreateStudentAsync(new CreateStudentRequestDto
        {
            FullName = "Unauthorized Creation",
            Email = "unauth@student.crm.edu"
        }, studentAccess);

        Assert.False(result.Succeeded);
        Assert.Equal(ServiceErrorType.Forbidden, result.ErrorType);
    }

    [Fact]
    public async Task GetStudentByIdAsync_EnforcesOwnership_BlocksStudentAccessingOtherStudent()
    {
        var (_, service) = CreateSut();
        var adminAccess = AccessContext.ForAdmin(1);

        var student1 = (await service.CreateStudentAsync(new CreateStudentRequestDto
        {
            FullName = "Student One",
            Email = "one@student.crm.edu",
            CreateLoginAccount = true
        }, adminAccess)).Data!;

        var student2 = (await service.CreateStudentAsync(new CreateStudentRequestDto
        {
            FullName = "Student Two",
            Email = "two@student.crm.edu",
            CreateLoginAccount = true
        }, adminAccess)).Data!;

        var student1Access = AccessContext.ForStudent(student1.UserId!.Value, student1.Id);

        var ownLookup = await service.GetStudentByIdAsync(student1.Id, student1Access);
        var otherLookup = await service.GetStudentByIdAsync(student2.Id, student1Access);
        var adminLookup = await service.GetStudentByIdAsync(student2.Id, adminAccess);

        Assert.True(ownLookup.Succeeded);
        Assert.False(otherLookup.Succeeded);
        Assert.Equal(ServiceErrorType.Forbidden, otherLookup.ErrorType);
        Assert.True(adminLookup.Succeeded);
    }

    [Fact]
    public async Task UpdatePersonalProfileAsync_OwnerCanEditPermittedFields_CannotEditOtherStudent()
    {
        var (_, service) = CreateSut();
        var adminAccess = AccessContext.ForAdmin(1);

        var student1 = (await service.CreateStudentAsync(new CreateStudentRequestDto
        {
            FullName = "Madina Aliyeva",
            Email = "madina@student.crm.edu",
            CreateLoginAccount = true
        }, adminAccess)).Data!;

        var student2 = (await service.CreateStudentAsync(new CreateStudentRequestDto
        {
            FullName = "Yerlan Sadykov",
            Email = "yerlan@student.crm.edu",
            CreateLoginAccount = true
        }, adminAccess)).Data!;

        var student1Access = AccessContext.ForStudent(student1.UserId!.Value, student1.Id);

        var ownUpdate = await service.UpdatePersonalProfileAsync(student1.Id, new UpdatePersonalProfileRequestDto
        {
            FullName = "Madina Aliyeva-Updated",
            PhoneNumber = "+7 701 999 8877",
            University = "Astana IT University",
            Faculty = "School of Data Science",
            Department = "Big Data",
            StudyProgram = "B.Sc. Big Data Analysis",
            StudyYear = 4,
            Bio = "Updated personal bio."
        }, student1Access);

        var crossUpdate = await service.UpdatePersonalProfileAsync(student2.Id, new UpdatePersonalProfileRequestDto
        {
            FullName = "Tampered Name",
            University = "Astana IT University",
            Faculty = "School of Data Science",
            Department = "Big Data",
            StudyProgram = "B.Sc. Big Data Analysis",
            StudyYear = 2
        }, student1Access);

        Assert.True(ownUpdate.Succeeded);
        Assert.Equal("Madina Aliyeva-Updated", ownUpdate.Data!.FullName);
        Assert.Equal(4, ownUpdate.Data.StudyYear);

        Assert.False(crossUpdate.Succeeded);
        Assert.Equal(ServiceErrorType.Forbidden, crossUpdate.ErrorType);
    }

    [Fact]
    public async Task GetStudentsAsync_SearchAndFilter_ReturnsMatchingRecords()
    {
        var (_, service) = CreateSut();
        var admin = AccessContext.ForAdmin(1);

        await service.CreateStudentAsync(new CreateStudentRequestDto
        {
            FullName = "Askar Zhumabayev",
            Email = "askar@student.crm.edu",
            StudentNumber = "STU-2026-0101",
            Faculty = "School of Cyber Security",
            Department = "Network Security",
            StudyProgram = "Cyber Security",
            StudyYear = 2,
            Status = StudentStatus.Active
        }, admin);

        await service.CreateStudentAsync(new CreateStudentRequestDto
        {
            FullName = "Zarina Mukanova",
            Email = "zarina@student.crm.edu",
            StudentNumber = "STU-2026-0102",
            Faculty = "School of Software Engineering",
            Department = "Software Systems",
            StudyProgram = "Software Engineering",
            StudyYear = 4,
            Status = StudentStatus.Inactive
        }, admin);

        var searchByName = await service.GetStudentsAsync(new StudentFilterDto { SearchTerm = "Zarina" }, admin);
        var filterByFaculty = await service.GetStudentsAsync(new StudentFilterDto { Faculty = "School of Cyber Security" }, admin);
        var filterByStatus = await service.GetStudentsAsync(new StudentFilterDto { Status = StudentStatus.Inactive }, admin);

        Assert.Single(searchByName.Data!);
        Assert.Equal("Zarina Mukanova", searchByName.Data![0].FullName);

        Assert.Single(filterByFaculty.Data!);
        Assert.Equal("Askar Zhumabayev", filterByFaculty.Data![0].FullName);

        Assert.Single(filterByStatus.Data!);
        Assert.Equal("STU-2026-0102", filterByStatus.Data![0].StudentNumber);
    }

    [Fact]
    public async Task SetStudentStatusAndDelete_AdminOperations_WorkAndHandleMissingIds()
    {
        var (_, service) = CreateSut();
        var admin = AccessContext.ForAdmin(1);

        var created = (await service.CreateStudentAsync(new CreateStudentRequestDto
        {
            FullName = "Temp Student",
            Email = "temp@student.crm.edu"
        }, admin)).Data!;

        var deactivateResult = await service.SetStudentStatusAsync(created.Id, StudentStatus.Inactive, admin);
        Assert.True(deactivateResult.Succeeded);
        Assert.Equal(StudentStatus.Inactive, deactivateResult.Data!.Status);

        var deleteResult = await service.DeleteStudentAsync(created.Id, admin);
        Assert.True(deleteResult.Succeeded);

        var missingDelete = await service.DeleteStudentAsync(created.Id, admin);
        Assert.False(missingDelete.Succeeded);
        Assert.Equal(ServiceErrorType.NotFound, missingDelete.ErrorType);
    }
}
