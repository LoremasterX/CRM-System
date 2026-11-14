using CRM_System.Core.Academic.DTOs;
using CRM_System.Core.Academic.Models;
using CRM_System.Core.Students.Models;
using CRM_System.Core.Tasks.DTOs;
using CRM_System.Core.Tasks.Models;

namespace CRM_System.Core.Dashboard.DTOs;

public class DashboardFilterDto
{
    public int? StudentId { get; set; }
    public int? CourseId { get; set; }
    public string? Semester { get; set; }
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
}

public sealed record DashboardKpiSummaryDto(
    bool IsStudentScoped,
    int? ScopedStudentId,
    string? ScopedStudentName,
    int TotalStudents,
    int ActiveStudents,
    int TotalCourses,
    int ActiveCourses,
    int TotalTeachers,
    int TotalEnrollments,
    decimal AverageGpa,
    decimal MaxGpaPoints,
    decimal OverallScorePercentage,
    decimal OverallAttendancePercentage,
    int TotalTasks,
    int ActiveTasks,
    int CompletedTasks,
    int OverdueTasks,
    int DueSoonTasks,
    int UnreadNotificationsCount);

public sealed record RecentAcademicActivityDto(
    string Category,
    string Title,
    string Subtitle,
    string BadgeText,
    string BadgeVariant,
    DateTime OccurredAt,
    int? StudentId,
    string? StudentName,
    int? CourseId,
    string? CourseCode);

public sealed record GpaDistributionBucketDto(
    string Label,
    string RangeDescription,
    decimal MinGpa,
    decimal MaxGpa,
    int StudentCount,
    decimal PercentageOfStudents,
    string ColorVariant);

public sealed record GradeDistributionBucketDto(
    string LetterGrade,
    string Description,
    decimal GradePoints,
    int AssessmentCount,
    decimal PercentageOfAssessments,
    string ColorVariant);

public sealed record CoursePerformanceAnalyticsDto(
    int CourseId,
    string CourseCode,
    string CourseTitle,
    string Department,
    string Semester,
    int Credits,
    string? TeacherName,
    bool IsActive,
    int EnrolledStudentsCount,
    int GradedAssessmentsCount,
    decimal AverageScorePercentage,
    decimal AverageGradePoints,
    string LetterGrade,
    decimal PassRatePercentage,
    decimal AttendanceRatePercentage,
    int TotalTasksCount,
    int CompletedTasksCount);

public sealed record StudentAcademicRankingDto(
    int StudentId,
    string StudentNumber,
    string StudentName,
    string StudyProgram,
    int StudyYear,
    StudentStatus Status,
    int EnrolledCoursesCount,
    int GradedCoursesCount,
    int TotalCreditsEarned,
    decimal CumulativeGpa,
    decimal OverallAveragePercentage,
    string StandingLabel,
    string StandingVariant,
    decimal AttendanceRatePercentage,
    int CompletedTasksCount,
    int OverdueTasksCount);

public sealed record AcademicProgressOverviewDto(
    int TotalEnrolledCourses,
    int GradedCoursesCount,
    int TotalCreditsEnrolled,
    int TotalCreditsEarned,
    decimal CreditCompletionPercentage,
    int TotalAssessmentsRecorded,
    decimal PassingAssessmentsPercentage,
    int DeansListStudentsCount,
    int AtRiskStudentsCount);

public sealed record AcademicAnalyticsDto(
    decimal AverageGpa,
    decimal MedianGpa,
    decimal HighestGpa,
    decimal LowestGpa,
    decimal MaxGpaPoints,
    decimal OverallAverageScorePercentage,
    int TotalGradedStudents,
    int TotalAssessmentsCount,
    StudentAcademicRankingDto? HighestPerformer,
    StudentAcademicRankingDto? LowestPerformer,
    CoursePerformanceAnalyticsDto? TopPerformingCourse,
    CoursePerformanceAnalyticsDto? LowestPerformingCourse,
    AcademicProgressOverviewDto ProgressOverview,
    IReadOnlyList<GpaDistributionBucketDto> GpaDistribution,
    IReadOnlyList<GradeDistributionBucketDto> GradeDistribution,
    IReadOnlyList<CoursePerformanceAnalyticsDto> CoursePerformances,
    IReadOnlyList<StudentAcademicRankingDto> StudentPerformances);

public sealed record AttendanceStatusSliceDto(
    AttendanceStatus Status,
    string Label,
    int Count,
    decimal Percentage,
    string ColorVariant);

public sealed record CourseAttendanceAnalyticsDto(
    int CourseId,
    string CourseCode,
    string CourseTitle,
    string Semester,
    string? TeacherName,
    int TotalSessions,
    int PresentCount,
    int LateCount,
    int AbsentCount,
    decimal AttendanceRatePercentage,
    decimal StrictPresentPercentage);

public sealed record StudentAttendanceAnalyticsDto(
    int StudentId,
    string StudentNumber,
    string StudentName,
    string StudyProgram,
    int StudyYear,
    int TotalSessions,
    int PresentCount,
    int LateCount,
    int AbsentCount,
    decimal AttendanceRatePercentage,
    decimal StrictPresentPercentage,
    bool IsAtRisk);

public sealed record AttendanceTrendPointDto(
    DateOnly PeriodStartDate,
    string PeriodLabel,
    int TotalSessions,
    int PresentCount,
    int LateCount,
    int AbsentCount,
    decimal AttendanceRatePercentage);

public sealed record AttendanceAnalyticsDto(
    int TotalSessions,
    int PresentCount,
    int LateCount,
    int AbsentCount,
    decimal OverallAttendancePercentage,
    decimal StrictPresentPercentage,
    int AtRiskStudentsCount,
    IReadOnlyList<AttendanceStatusSliceDto> StatusDistribution,
    IReadOnlyList<CourseAttendanceAnalyticsDto> CourseStatistics,
    IReadOnlyList<StudentAttendanceAnalyticsDto> StudentStatistics,
    IReadOnlyList<AttendanceTrendPointDto> AttendanceTrends);

public sealed record TaskPriorityAnalyticsDto(
    TaskPriority Priority,
    string Label,
    int TotalCount,
    int PendingCount,
    int InProgressCount,
    int CompletedCount,
    int OverdueCount,
    decimal PercentageOfTotal,
    decimal CompletionRatePercentage,
    string ColorVariant);

public sealed record TaskStatusAnalyticsDto(
    StudentTaskStatus Status,
    string Label,
    int Count,
    decimal Percentage,
    string ColorVariant);

public sealed record CourseTaskAnalyticsDto(
    int? CourseId,
    string CourseCode,
    string CourseTitle,
    int TotalTasks,
    int CompletedTasks,
    int PendingTasks,
    int InProgressTasks,
    int OverdueTasks,
    decimal CompletionRatePercentage);

public sealed record TaskProductivityAnalyticsDto(
    int TotalTasks,
    int CompletedTasks,
    int PendingTasks,
    int InProgressTasks,
    int ActiveTasks,
    int OverdueTasks,
    int DueSoonTasks,
    decimal CompletionRatePercentage,
    decimal OnTimeCompletionRatePercentage,
    decimal AverageTasksPerStudent,
    IReadOnlyList<TaskPriorityAnalyticsDto> PriorityBreakdown,
    IReadOnlyList<TaskStatusAnalyticsDto> StatusBreakdown,
    IReadOnlyList<CourseTaskAnalyticsDto> CourseBreakdown,
    IReadOnlyList<StudentTaskDto> UpcomingDeadlines,
    IReadOnlyList<StudentTaskDto> OverdueTaskItems);

public sealed record DashboardOverviewDto(
    DashboardFilterDto AppliedFilter,
    IReadOnlyList<string> AvailableSemesters,
    DashboardKpiSummaryDto Kpis,
    AcademicAnalyticsDto AcademicAnalytics,
    AttendanceAnalyticsDto AttendanceAnalytics,
    TaskProductivityAnalyticsDto TaskAnalytics,
    IReadOnlyList<StudentTaskDto> UpcomingDeadlines,
    IReadOnlyList<NotificationDto> RecentNotifications,
    IReadOnlyList<RecentAcademicActivityDto> RecentActivity);
