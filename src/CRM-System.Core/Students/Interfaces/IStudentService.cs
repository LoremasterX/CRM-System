using CRM_System.Core.Common;
using CRM_System.Core.Students.DTOs;
using CRM_System.Core.Students.Models;

namespace CRM_System.Core.Students.Interfaces;

public interface IStudentService
{
    Task<ServiceResult<IReadOnlyList<StudentDto>>> GetStudentsAsync(StudentFilterDto? filter, AccessContext access, CancellationToken cancellationToken = default);
    Task<ServiceResult<StudentDto>> GetStudentByIdAsync(int id, AccessContext access, CancellationToken cancellationToken = default);
    Task<ServiceResult<StudentDto>> GetStudentByUserIdAsync(int userId, AccessContext access, CancellationToken cancellationToken = default);
    Task<ServiceResult<StudentDto>> CreateStudentAsync(CreateStudentRequestDto request, AccessContext access, CancellationToken cancellationToken = default);
    Task<ServiceResult<StudentDto>> UpdateStudentAsync(int id, UpdateStudentRequestDto request, AccessContext access, CancellationToken cancellationToken = default);
    Task<ServiceResult<StudentDto>> UpdatePersonalProfileAsync(int studentId, UpdatePersonalProfileRequestDto request, AccessContext access, CancellationToken cancellationToken = default);
    Task<ServiceResult<StudentDto>> SetStudentStatusAsync(int id, StudentStatus status, AccessContext access, CancellationToken cancellationToken = default);
    Task<ServiceResult> DeleteStudentAsync(int id, AccessContext access, CancellationToken cancellationToken = default);
    Task<ServiceResult<StudentDirectoryStatsDto>> GetDirectoryStatsAsync(AccessContext access, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetDistinctFacultiesAsync(CancellationToken cancellationToken = default);
}
