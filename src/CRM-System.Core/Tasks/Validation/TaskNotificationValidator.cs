using CRM_System.Core.Tasks.DTOs;

namespace CRM_System.Core.Tasks.Validation;

public static class TaskNotificationValidator
{
    public static List<string> ValidateTask(SaveStudentTaskRequestDto request)
    {
        var errors = new List<string>();

        if (request.StudentId <= 0)
        {
            errors.Add("A valid student must be assigned to the task.");
        }

        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Trim().Length < 3)
        {
            errors.Add("Task title must be at least 3 characters long.");
        }
        else if (request.Title.Trim().Length > 150)
        {
            errors.Add("Task title cannot exceed 150 characters.");
        }

        if (string.IsNullOrWhiteSpace(request.Description) || request.Description.Trim().Length < 3)
        {
            errors.Add("Task description must be at least 3 characters long.");
        }
        else if (request.Description.Trim().Length > 1000)
        {
            errors.Add("Task description cannot exceed 1000 characters.");
        }

        if (request.DueDate.Year < 2020 || request.DueDate.Year > 2100)
        {
            errors.Add("Task deadline date must be between years 2020 and 2100.");
        }

        return errors;
    }

    public static List<string> ValidateNotification(CreateNotificationRequestDto request)
    {
        var errors = new List<string>();

        if (!request.StudentId.HasValue && !request.UserId.HasValue)
        {
            errors.Add("Notification must target either a student or a user account.");
        }

        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Trim().Length < 3)
        {
            errors.Add("Notification title must be at least 3 characters long.");
        }

        if (string.IsNullOrWhiteSpace(request.Message) || request.Message.Trim().Length < 3)
        {
            errors.Add("Notification message must be at least 3 characters long.");
        }

        return errors;
    }
}
