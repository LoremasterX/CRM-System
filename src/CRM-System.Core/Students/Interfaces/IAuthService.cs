using CRM_System.Core.Common;
using CRM_System.Core.Students.DTOs;

namespace CRM_System.Core.Students.Interfaces;

public interface IAuthService
{
    Task<ServiceResult<AuthenticatedUserDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<ServiceResult<AuthenticatedUserDto>> RegisterStudentAsync(RegisterStudentRequestDto request, CancellationToken cancellationToken = default);
    Task<ServiceResult> ChangePasswordAsync(int userId, ChangePasswordRequestDto request, CancellationToken cancellationToken = default);
    Task<AuthenticatedUserDto?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default);
}
