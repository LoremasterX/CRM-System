namespace CRM_System.Core.Academic.Models;

public class ScheduleEntry
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public DayOfWeek DayOfWeek { get; set; } = DayOfWeek.Monday;
    public TimeOnly StartTime { get; set; } = new(9, 0);
    public TimeOnly EndTime { get; set; } = new(10, 30);
    public string Classroom { get; set; } = string.Empty;
    public string LessonType { get; set; } = "Lecture";

    public Course? Course { get; set; }
}
