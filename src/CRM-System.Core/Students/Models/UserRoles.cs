namespace CRM_System.Core.Students.Models;

public static class UserRoles
{
    public const string Admin = "Admin";
    public const string Student = "Student";

    public static bool IsValidRole(string? role) =>
        string.Equals(role, Admin, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(role, Student, StringComparison.OrdinalIgnoreCase);

    public static string Normalize(string? role) =>
        string.Equals(role, Admin, StringComparison.OrdinalIgnoreCase) ? Admin : Student;
}
