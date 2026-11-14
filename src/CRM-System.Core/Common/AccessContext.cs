using CRM_System.Core.Students.Models;

namespace CRM_System.Core.Common;

public sealed record AccessContext(
    int? UserId,
    int? StudentId,
    string? Role,
    bool IsAuthenticated)
{
    public bool IsAdmin => IsAuthenticated && string.Equals(Role, UserRoles.Admin, StringComparison.OrdinalIgnoreCase);
    public bool IsStudent => IsAuthenticated && string.Equals(Role, UserRoles.Student, StringComparison.OrdinalIgnoreCase);

    public static AccessContext Anonymous => new(null, null, null, false);

    public static AccessContext ForAdmin(int userId = 1) => new(userId, null, UserRoles.Admin, true);

    public static AccessContext ForStudent(int userId, int? studentId) => new(userId, studentId, UserRoles.Student, true);

    public bool CanAccessStudent(int targetStudentId, int? targetStudentUserId = null)
    {
        if (!IsAuthenticated)
        {
            return false;
        }

        if (IsAdmin)
        {
            return true;
        }

        if (StudentId.HasValue && StudentId.Value == targetStudentId)
        {
            return true;
        }

        if (UserId.HasValue && targetStudentUserId.HasValue && UserId.Value == targetStudentUserId.Value)
        {
            return true;
        }

        return false;
    }
}
