using CRM_System.Core.Students.DTOs;
using CRM_System.Core.Students.Models;

namespace CRM_System.Core.Students.Interfaces;

public interface IStudentRepository
{
    Task<IReadOnlyList<Student>> GetStudentsAsync(StudentFilterDto? filter = null, CancellationToken cancellationToken = default);
    Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Student?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<Student?> GetByStudentNumberAsync(string studentNumber, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, int? excludeStudentId = null, CancellationToken cancellationToken = default);
    Task<bool> StudentNumberExistsAsync(string studentNumber, int? excludeStudentId = null, CancellationToken cancellationToken = default);
    Task<string> GenerateNextStudentNumberAsync(CancellationToken cancellationToken = default);
    Task<Student> AddAsync(Student student, CancellationToken cancellationToken = default);
    Task UpdateAsync(Student student, CancellationToken cancellationToken = default);
    Task DeleteAsync(Student student, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetDistinctFacultiesAsync(CancellationToken cancellationToken = default);
}
