using CRM_System.Core.Students.DTOs;
using CRM_System.Core.Students.Interfaces;
using CRM_System.Core.Students.Models;
using CRM_System.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CRM_System.Infrastructure.Students;

public class StudentRepository : IStudentRepository
{
    private readonly CrmDbContext _dbContext;

    public StudentRepository(CrmDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Student>> GetStudentsAsync(
        StudentFilterDto? filter = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Student> query = _dbContext.Students.AsNoTracking();

        if (filter is not null)
        {
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim().ToLowerInvariant();
                query = query.Where(s =>
                    s.FullName.ToLower().Contains(term) ||
                    s.Email.ToLower().Contains(term) ||
                    s.StudentNumber.ToLower().Contains(term) ||
                    s.Department.ToLower().Contains(term) ||
                    s.StudyProgram.ToLower().Contains(term) ||
                    s.University.ToLower().Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(filter.Faculty))
            {
                var faculty = filter.Faculty.Trim();
                query = query.Where(s => s.Faculty == faculty);
            }

            if (filter.StudyYear.HasValue)
            {
                query = query.Where(s => s.StudyYear == filter.StudyYear.Value);
            }

            if (filter.Status.HasValue)
            {
                query = query.Where(s => s.Status == filter.Status.Value);
            }

            query = (filter.SortBy?.ToLowerInvariant(), filter.SortDescending) switch
            {
                ("studentnumber", true) => query.OrderByDescending(s => s.StudentNumber),
                ("studentnumber", false) => query.OrderBy(s => s.StudentNumber),
                ("studyyear", true) => query.OrderByDescending(s => s.StudyYear).ThenBy(s => s.FullName),
                ("studyyear", false) => query.OrderBy(s => s.StudyYear).ThenBy(s => s.FullName),
                ("faculty", true) => query.OrderByDescending(s => s.Faculty).ThenBy(s => s.FullName),
                ("faculty", false) => query.OrderBy(s => s.Faculty).ThenBy(s => s.FullName),
                ("enrollmentdate", true) => query.OrderByDescending(s => s.EnrollmentDate),
                ("enrollmentdate", false) => query.OrderBy(s => s.EnrollmentDate),
                (_, true) => query.OrderByDescending(s => s.FullName),
                _ => query.OrderBy(s => s.FullName)
            };
        }
        else
        {
            query = query.OrderBy(s => s.FullName);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _dbContext.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public Task<Student?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default) =>
        _dbContext.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);

    public Task<Student?> GetByStudentNumberAsync(string studentNumber, CancellationToken cancellationToken = default)
    {
        var normalized = studentNumber.Trim().ToUpperInvariant();
        return _dbContext.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.StudentNumber.ToUpper() == normalized, cancellationToken);
    }

    public Task<bool> EmailExistsAsync(string email, int? excludeStudentId = null, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return _dbContext.Students.AnyAsync(
            s => s.Email.ToLower() == normalized && (!excludeStudentId.HasValue || s.Id != excludeStudentId.Value),
            cancellationToken);
    }

    public Task<bool> StudentNumberExistsAsync(string studentNumber, int? excludeStudentId = null, CancellationToken cancellationToken = default)
    {
        var normalized = studentNumber.Trim().ToUpperInvariant();
        return _dbContext.Students.AnyAsync(
            s => s.StudentNumber.ToUpper() == normalized && (!excludeStudentId.HasValue || s.Id != excludeStudentId.Value),
            cancellationToken);
    }

    public async Task<string> GenerateNextStudentNumberAsync(CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"STU-{year}-";
        var existingNumbers = await _dbContext.Students
            .Where(s => s.StudentNumber.StartsWith(prefix))
            .Select(s => s.StudentNumber)
            .ToListAsync(cancellationToken);

        var maxSequence = 0;
        foreach (var number in existingNumbers)
        {
            var suffix = number[prefix.Length..];
            if (int.TryParse(suffix, out var seq) && seq > maxSequence)
            {
                maxSequence = seq;
            }
        }

        return $"{prefix}{maxSequence + 1:D4}";
    }

    public async Task<Student> AddAsync(Student student, CancellationToken cancellationToken = default)
    {
        _dbContext.Students.Add(student);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return student;
    }

    public async Task UpdateAsync(Student student, CancellationToken cancellationToken = default)
    {
        _dbContext.Students.Update(student);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Student student, CancellationToken cancellationToken = default)
    {
        _dbContext.Students.Remove(student);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetDistinctFacultiesAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Students
            .AsNoTracking()
            .Select(s => s.Faculty)
            .Distinct()
            .OrderBy(f => f)
            .ToListAsync(cancellationToken);
}
