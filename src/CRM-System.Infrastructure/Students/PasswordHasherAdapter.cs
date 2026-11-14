using CRM_System.Core.Students.Interfaces;
using CRM_System.Core.Students.Models;
using Microsoft.AspNetCore.Identity;

namespace CRM_System.Infrastructure.Students;

public class PasswordHasherAdapter : IPasswordHasher
{
    private readonly PasswordHasher<ApplicationUser> _innerHasher = new();
    private static readonly ApplicationUser DummyUser = new();

    public string HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        return _innerHasher.HashPassword(DummyUser, password);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
        {
            return false;
        }

        var result = _innerHasher.VerifyHashedPassword(DummyUser, passwordHash, password);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
