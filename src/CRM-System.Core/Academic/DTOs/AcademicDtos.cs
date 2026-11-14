using System.ComponentModel.DataAnnotations;
using CRM_System.Core.Academic.Models;

namespace CRM_System.Core.Academic.DTOs;

public sealed record TeacherDto(
    int Id,
    string FullName,
    string Email,
    string Department,
    string AcademicTitle,
    string? OfficeRoom,
    string? PhoneNumber,
    string? Bio,
    int AssignedCoursesCount,
    IReadOnlyList<string> CourseCodes);

public class SaveTeacherRequestDto
{
    [Required(ErrorMessage = "Teacher full name is required.")]
    [StringLength(120, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Teacher email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    [StringLength(160)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Department is required.")]
    [StringLength(120, MinimumLength = 2)]
    public string Department { get; set; } = "Computer Science & Engineering";

    [Required(ErrorMessage = "Academic title is required.")]
    [StringLength(80, MinimumLength = 2)]
    public string AcademicTitle { get; set; } = "Associate Professor";

    [StringLength(40)]
    public string? OfficeRoom { get; set; }

    [StringLength(32)]
    public string? PhoneNumber { get; set; }

    [StringLength(1000)]
    public string? Bio { get; set; }
}

public sealed record CourseDto(
    int Id,
    string CourseCode,
    string Title,
    string Description,
    int Credits,
    string Department,
    string Semester,
    int? TeacherId,
    string? TeacherName,
    string? TeacherTitle,
    bool IsActive,
    int EnrolledStudentsCount);

public class SaveCourseRequestDto
{
    [Required(ErrorMessage = "Course code is required.")]
    [StringLength(24, MinimumLength = 3, ErrorMessage = "Course code must be between 3 and 24 characters.")]
    public string CourseCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Course title is required.")]
    [StringLength(150, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Course description is required.")]
    [StringLength(1000, MinimumLength = 5)]
    public string Description { get; set; } = string.Empty;

    [Range(1, 15, ErrorMessage = "Course credits must be between 1 and 15.")]
    public int Credits { get; set; } = 5;

    [Required(ErrorMessage = "Department is required.")]
    [StringLength(120, MinimumLength = 2)]
    public string Department { get; set; } = "Computer Science & Engineering";

    [Required(ErrorMessage = "Semester is required.")]
    [StringLength(40, MinimumLength = 3)]
    public string Semester { get; set; } = "Fall 2026";

    public int? TeacherId { get; set; }

    public bool IsActive { get; set; } = true;
}

public sealed record EnrollmentDto(
    int Id,
    int StudentId,
    string StudentName,
    string StudentNumber,
    int CourseId,
    string CourseCode,
    string CourseTitle,
    int Credits,
    string? TeacherName,
    string Semester,
    EnrollmentStatus Status,
    DateTime EnrolledAt);

public class CreateEnrollmentRequestDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Valid student is required.")]
    public int StudentId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Valid course is required.")]
    public int CourseId { get; set; }

    [Required]
    [StringLength(40)]
    public string Semester { get; set; } = "Fall 2026";
}

public sealed record GradeDto(
    int Id,
    int StudentId,
    string StudentName,
    string StudentNumber,
    int CourseId,
    string CourseCode,
    string CourseTitle,
    int CourseCredits,
    string AssessmentTitle,
    string AssessmentType,
    decimal Score,
    decimal Weight,
    string LetterGrade,
    decimal GradePoints,
    string? Feedback,
    DateTime GradedAt);

public class SaveGradeRequestDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Please select a student.")]
    public int StudentId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Please select a course.")]
    public int CourseId { get; set; }

    [Required(ErrorMessage = "Assessment title is required.")]
    [StringLength(120, MinimumLength = 2)]
    public string AssessmentTitle { get; set; } = string.Empty;

    [Required(ErrorMessage = "Assessment type is required.")]
    [StringLength(40)]
    public string AssessmentType { get; set; } = "Exam";

    [Range(0, 100, ErrorMessage = "Score must be between 0 and 100.")]
    public decimal Score { get; set; } = 85.0m;

    [Range(0.05, 5.0, ErrorMessage = "Weight must be between 0.05 and 5.0.")]
    public decimal Weight { get; set; } = 1.0m;

    [StringLength(500)]
    public string? Feedback { get; set; }
}

public sealed record CourseGradeSummaryDto(
    int CourseId,
    string CourseCode,
    string CourseTitle,
    int Credits,
    int AssessmentsCount,
    decimal AveragePercentage,
    string LetterGrade,
    decimal GradePoints,
    IReadOnlyList<GradeDto> Assessments);

public sealed record StudentGpaSummaryDto(
    int StudentId,
    string StudentName,
    string StudentNumber,
    string ScaleName,
    decimal MaxGpaPoints,
    decimal CumulativeGpa,
    decimal OverallAveragePercentage,
    int GradedCoursesCount,
    int TotalCreditsEarned,
    IReadOnlyList<CourseGradeSummaryDto> CourseSummaries,
    IReadOnlyList<GradingScaleBand> ScaleBands);

public sealed record ScheduleEntryDto(
    int Id,
    int CourseId,
    string CourseCode,
    string CourseTitle,
    string? TeacherName,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string Classroom,
    string LessonType);

public class SaveScheduleEntryRequestDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Please select a course.")]
    public int CourseId { get; set; }

    public DayOfWeek DayOfWeek { get; set; } = DayOfWeek.Monday;

    public TimeOnly StartTime { get; set; } = new(9, 0);

    public TimeOnly EndTime { get; set; } = new(10, 30);

    [Required(ErrorMessage = "Classroom is required.")]
    [StringLength(60, MinimumLength = 2)]
    public string Classroom { get; set; } = "C1.2.245";

    [Required(ErrorMessage = "Lesson type is required.")]
    [StringLength(40)]
    public string LessonType { get; set; } = "Lecture";
}

public sealed record AttendanceRecordDto(
    int Id,
    int StudentId,
    string StudentName,
    string StudentNumber,
    int CourseId,
    string CourseCode,
    string CourseTitle,
    DateOnly Date,
    AttendanceStatus Status,
    string? Topic,
    string? Notes,
    DateTime RecordedAt);

public class SaveAttendanceRequestDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Please select a student.")]
    public int StudentId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Please select a course.")]
    public int CourseId { get; set; }

    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;

    [StringLength(150)]
    public string? Topic { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}

public sealed record CourseAttendanceSummaryDto(
    int CourseId,
    string CourseCode,
    string CourseTitle,
    int TotalSessions,
    int PresentCount,
    int LateCount,
    int AbsentCount,
    decimal AttendanceRatePercentage);

public sealed record StudentAttendanceStatsDto(
    int StudentId,
    int TotalSessions,
    int PresentCount,
    int LateCount,
    int AbsentCount,
    decimal AttendanceRatePercentage,
    decimal StrictPresentPercentage,
    IReadOnlyList<CourseAttendanceSummaryDto> CourseBreakdowns,
    IReadOnlyList<AttendanceRecordDto> Records);
