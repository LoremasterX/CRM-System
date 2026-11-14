using CRM_System.Core.Students.Models;

namespace CRM_System.Core.Students.Interfaces;

public interface IUserRepository
{
    Task<ApplicationUser?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, int? excludeUserId = null, CancellationToken cancellationToken = default);
    Task<ApplicationUser> AddAsync(ApplicationUser user, CancellationToken cancellationToken = default);
    Task UpdateAsync(ApplicationUser user, CancellationToken cancellationToken = default);
}
