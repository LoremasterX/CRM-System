using CRM_System.Core.Students.Interfaces;
using CRM_System.Core.Students.Models;
using CRM_System.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CRM_System.Infrastructure.Students;

public class UserRepository : IUserRepository
{
    private readonly CrmDbContext _dbContext;

    public UserRepository(CrmDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ApplicationUser?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _dbContext.Users
            .Include(u => u.StudentProfile)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToUpperInvariant();
        return _dbContext.Users
            .Include(u => u.StudentProfile)
            .FirstOrDefaultAsync(u => u.NormalizedEmail == normalized, cancellationToken);
    }

    public Task<bool> EmailExistsAsync(string email, int? excludeUserId = null, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToUpperInvariant();
        return _dbContext.Users.AnyAsync(
            u => u.NormalizedEmail == normalized && (!excludeUserId.HasValue || u.Id != excludeUserId.Value),
            cancellationToken);
    }

    public async Task<ApplicationUser> AddAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task UpdateAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
