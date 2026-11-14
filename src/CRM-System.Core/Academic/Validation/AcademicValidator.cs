using CRM_System.Core.Academic.DTOs;
using CRM_System.Core.Academic.Models;

namespace CRM_System.Core.Academic.Validation;

public static class AcademicValidator
{
    public static IReadOnlyList<string> ValidateCourse(SaveCourseRequestDto? request)
    {
        if (request is null)
        {
            return ["Course request cannot be null."];
        }

        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(request.CourseCode) || request.CourseCode.Trim().Length < 3)
        {
            errors.Add("Course code is required and must be at least 3 characters.");
        }

        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Trim().Length < 3)
        {
            errors.Add("Course title is required and must be at least 3 characters.");
        }

        if (string.IsNullOrWhiteSpace(request.Description) || request.Description.Trim().Length < 5)
        {
            errors.Add("Course description is required and must be at least 5 characters.");
        }

        if (request.Credits is < 1 or > 15)
        {
            errors.Add("Course credits must be between 1 and 15.");
        }

        if (string.IsNullOrWhiteSpace(request.Department))
        {
            errors.Add("Course department is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Semester))
        {
            errors.Add("Course semester is required.");
        }

        return errors;
    }

    public static IReadOnlyList<string> ValidateTeacher(SaveTeacherRequestDto? request)
    {
        if (request is null)
        {
            return ["Teacher request cannot be null."];
        }

        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(request.FullName) || request.FullName.Trim().Length < 2)
        {
            errors.Add("Teacher full name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
        {
            errors.Add("Valid teacher email is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Department))
        {
            errors.Add("Teacher department is required.");
        }

        if (string.IsNullOrWhiteSpace(request.AcademicTitle))
        {
            errors.Add("Teacher academic title is required.");
        }

        return errors;
    }

    public static IReadOnlyList<string> ValidateGrade(SaveGradeRequestDto? request)
    {
        if (request is null)
        {
            return ["Grade request cannot be null."];
        }

        var errors = new List<string>();
        if (request.StudentId <= 0)
        {
            errors.Add("A valid student must be selected.");
        }

        if (request.CourseId <= 0)
        {
            errors.Add("A valid course must be selected.");
        }

        if (string.IsNullOrWhiteSpace(request.AssessmentTitle) || request.AssessmentTitle.Trim().Length < 2)
        {
            errors.Add("Assessment title is required.");
        }

        if (request.Score is < 0m or > 100m)
        {
            errors.Add("Score must be between 0 and 100.");
        }

        if (request.Weight is <= 0m or > 5.0m)
        {
            errors.Add("Grade weight must be greater than 0 and at most 5.0.");
        }

        return errors;
    }

    public static IReadOnlyList<string> ValidateScheduleEntry(SaveScheduleEntryRequestDto? request)
    {
        if (request is null)
        {
            return ["Schedule entry request cannot be null."];
        }

        var errors = new List<string>();
        if (request.CourseId <= 0)
        {
            errors.Add("A valid course must be selected.");
        }

        if (request.EndTime <= request.StartTime)
        {
            errors.Add("Class end time must be after start time.");
        }

        if (string.IsNullOrWhiteSpace(request.Classroom))
        {
            errors.Add("Classroom location is required.");
        }

        if (string.IsNullOrWhiteSpace(request.LessonType))
        {
            errors.Add("Lesson type is required.");
        }

        return errors;
    }

    public static IReadOnlyList<string> ValidateAttendance(SaveAttendanceRequestDto? request)
    {
        if (request is null)
        {
            return ["Attendance request cannot be null."];
        }

        var errors = new List<string>();
        if (request.StudentId <= 0)
        {
            errors.Add("A valid student must be selected.");
        }

        if (request.CourseId <= 0)
        {
            errors.Add("A valid course must be selected.");
        }

        if (!Enum.IsDefined(typeof(AttendanceStatus), request.Status))
        {
            errors.Add("Invalid attendance status.");
        }

        return errors;
    }
}
