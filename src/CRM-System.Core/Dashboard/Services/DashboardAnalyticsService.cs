using CRM_System.Core.Academic.DTOs;
using CRM_System.Core.Academic.Interfaces;
using CRM_System.Core.Academic.Models;
using CRM_System.Core.Common;
using CRM_System.Core.Dashboard.DTOs;
using CRM_System.Core.Dashboard.Interfaces;
using CRM_System.Core.Students.Models;
using CRM_System.Core.Tasks.DTOs;
using CRM_System.Core.Tasks.Models;

namespace CRM_System.Core.Dashboard.Services;

public class DashboardAnalyticsService : IDashboardAnalyticsService
{
    private readonly IDashboardAnalyticsRepository _repository;
    private readonly IGpaCalculator _gpaCalculator;

    public DashboardAnalyticsService(
        IDashboardAnalyticsRepository repository,
        IGpaCalculator gpaCalculator)
    {
        _repository = repository;
        _gpaCalculator = gpaCalculator;
    }

    public async Task<ServiceResult<DashboardOverviewDto>> GetDashboardOverviewAsync(
        DashboardFilterDto? filter,
        AccessContext access,
        DateTime? referenceUtcNow = null,
        CancellationToken cancellationToken = default)
    {
        var filterResolution = ResolveEffectiveFilter(filter, access);
        if (!filterResolution.Succeeded || filterResolution.Data is null)
        {
            return PropagateFailure<DashboardOverviewDto>(filterResolution);
        }

        var effectiveFilter = filterResolution.Data;
        var utcNow = referenceUtcNow ?? DateTime.UtcNow;

        var snapshot = await _repository.GetDatasetSnapshotAsync(effectiveFilter.StudentId, cancellationToken);
        var availableSemesters = ExtractAvailableSemesters(snapshot);
        var filtered = ApplyFilters(snapshot, effectiveFilter, access);

        var academic = BuildAcademicAnalytics(filtered);
        var attendance = BuildAttendanceAnalytics(filtered);
        var tasks = BuildTaskProductivityAnalytics(filtered, utcNow);

        var scopedStudent = effectiveFilter.StudentId.HasValue
            ? filtered.Students.FirstOrDefault(s => s.Id == effectiveFilter.StudentId.Value)
              ?? snapshot.Students.FirstOrDefault(s => s.Id == effectiveFilter.StudentId.Value)
            : null;

        var unreadNotifications = filtered.Notifications.Count(n => !n.IsRead);

        var kpis = new DashboardKpiSummaryDto(
            IsStudentScoped: !access.IsAdmin || effectiveFilter.StudentId.HasValue,
            ScopedStudentId: effectiveFilter.StudentId,
            ScopedStudentName: scopedStudent?.FullName,
            TotalStudents: filtered.Students.Count,
            ActiveStudents: filtered.Students.Count(s => s.Status == StudentStatus.Active),
            TotalCourses: filtered.Courses.Count,
            ActiveCourses: filtered.Courses.Count(c => c.IsActive),
            TotalTeachers: filtered.Teachers.Count,
            TotalEnrollments: filtered.Enrollments.Count(e => e.Status == EnrollmentStatus.Enrolled),
            AverageGpa: academic.AverageGpa,
            MaxGpaPoints: academic.MaxGpaPoints,
            OverallScorePercentage: academic.OverallAverageScorePercentage,
            OverallAttendancePercentage: attendance.OverallAttendancePercentage,
            TotalTasks: tasks.TotalTasks,
            ActiveTasks: tasks.ActiveTasks,
            CompletedTasks: tasks.CompletedTasks,
            OverdueTasks: tasks.OverdueTasks,
            DueSoonTasks: tasks.DueSoonTasks,
            UnreadNotificationsCount: unreadNotifications);

        var recentNotifications = filtered.Notifications
            .OrderByDescending(n => n.CreatedAt)
            .ThenByDescending(n => n.Id)
            .Take(6)
            .Select(MapNotificationDto)
            .ToList();

        var recentActivity = BuildRecentActivityFeed(filtered, utcNow);

        var overview = new DashboardOverviewDto(
            AppliedFilter: effectiveFilter,
            AvailableSemesters: availableSemesters,
            Kpis: kpis,
            AcademicAnalytics: academic,
            AttendanceAnalytics: attendance,
            TaskAnalytics: tasks,
            UpcomingDeadlines: tasks.UpcomingDeadlines,
            RecentNotifications: recentNotifications,
            RecentActivity: recentActivity);

        return ServiceResult<DashboardOverviewDto>.Success(overview);
    }

    public async Task<ServiceResult<AcademicAnalyticsDto>> GetAcademicAnalyticsAsync(
        DashboardFilterDto? filter,
        AccessContext access,
        CancellationToken cancellationToken = default)
    {
        var filterResolution = ResolveEffectiveFilter(filter, access);
        if (!filterResolution.Succeeded || filterResolution.Data is null)
        {
            return PropagateFailure<AcademicAnalyticsDto>(filterResolution);
        }

        var effectiveFilter = filterResolution.Data;
        var snapshot = await _repository.GetDatasetSnapshotAsync(effectiveFilter.StudentId, cancellationToken);
        var filtered = ApplyFilters(snapshot, effectiveFilter, access);

        return ServiceResult<AcademicAnalyticsDto>.Success(BuildAcademicAnalytics(filtered));
    }

    public async Task<ServiceResult<AttendanceAnalyticsDto>> GetAttendanceAnalyticsAsync(
        DashboardFilterDto? filter,
        AccessContext access,
        CancellationToken cancellationToken = default)
    {
        var filterResolution = ResolveEffectiveFilter(filter, access);
        if (!filterResolution.Succeeded || filterResolution.Data is null)
        {
            return PropagateFailure<AttendanceAnalyticsDto>(filterResolution);
        }

        var effectiveFilter = filterResolution.Data;
        var snapshot = await _repository.GetDatasetSnapshotAsync(effectiveFilter.StudentId, cancellationToken);
        var filtered = ApplyFilters(snapshot, effectiveFilter, access);

        return ServiceResult<AttendanceAnalyticsDto>.Success(BuildAttendanceAnalytics(filtered));
    }

    public async Task<ServiceResult<TaskProductivityAnalyticsDto>> GetTaskProductivityAnalyticsAsync(
        DashboardFilterDto? filter,
        AccessContext access,
        DateTime? referenceUtcNow = null,
        CancellationToken cancellationToken = default)
    {
        var filterResolution = ResolveEffectiveFilter(filter, access);
        if (!filterResolution.Succeeded || filterResolution.Data is null)
        {
            return PropagateFailure<TaskProductivityAnalyticsDto>(filterResolution);
        }

        var effectiveFilter = filterResolution.Data;
        var utcNow = referenceUtcNow ?? DateTime.UtcNow;
        var snapshot = await _repository.GetDatasetSnapshotAsync(effectiveFilter.StudentId, cancellationToken);
        var filtered = ApplyFilters(snapshot, effectiveFilter, access);

        return ServiceResult<TaskProductivityAnalyticsDto>.Success(BuildTaskProductivityAnalytics(filtered, utcNow));
    }

    private static ServiceResult<DashboardFilterDto> ResolveEffectiveFilter(
        DashboardFilterDto? filter,
        AccessContext access)
    {
        if (!access.IsAuthenticated)
        {
            return ServiceResult<DashboardFilterDto>.Failure(
                "Authentication is required to view dashboard and analytics.",
                ServiceErrorType.Unauthorized);
        }

        var incoming = filter ?? new DashboardFilterDto();

        if (incoming.DateFrom.HasValue && incoming.DateTo.HasValue && incoming.DateFrom.Value > incoming.DateTo.Value)
        {
            return ServiceResult<DashboardFilterDto>.Failure(
                "Start date cannot be later than end date.",
                ServiceErrorType.Validation);
        }

        if (!access.IsAdmin)
        {
            if (!access.StudentId.HasValue)
            {
                return ServiceResult<DashboardFilterDto>.Failure(
                    "Student account is not linked to a student profile.",
                    ServiceErrorType.Forbidden);
            }

            if (incoming.StudentId.HasValue && incoming.StudentId.Value != access.StudentId.Value)
            {
                return ServiceResult<DashboardFilterDto>.Failure(
                    "You do not have permission to view another student's analytics.",
                    ServiceErrorType.Forbidden);
            }

            return ServiceResult<DashboardFilterDto>.Success(new DashboardFilterDto
            {
                StudentId = access.StudentId.Value,
                CourseId = incoming.CourseId,
                Semester = string.IsNullOrWhiteSpace(incoming.Semester) ? null : incoming.Semester.Trim(),
                DateFrom = incoming.DateFrom,
                DateTo = incoming.DateTo
            });
        }

        return ServiceResult<DashboardFilterDto>.Success(new DashboardFilterDto
        {
            StudentId = incoming.StudentId,
            CourseId = incoming.CourseId,
            Semester = string.IsNullOrWhiteSpace(incoming.Semester) ? null : incoming.Semester.Trim(),
            DateFrom = incoming.DateFrom,
            DateTo = incoming.DateTo
        });
    }

    private static IReadOnlyList<string> ExtractAvailableSemesters(DashboardDatasetSnapshot snapshot)
    {
        return snapshot.Courses
            .Select(c => c.Semester)
            .Concat(snapshot.Enrollments.Select(e => e.Semester))
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(s => s)
            .ToList();
    }

    private static DashboardDatasetSnapshot ApplyFilters(
        DashboardDatasetSnapshot snapshot,
        DashboardFilterDto filter,
        AccessContext access)
    {
        var students = snapshot.Students.AsEnumerable();
        var courses = snapshot.Courses.AsEnumerable();
        var enrollments = snapshot.Enrollments.AsEnumerable();
        var grades = snapshot.Grades.AsEnumerable();
        var attendance = snapshot.AttendanceRecords.AsEnumerable();
        var tasks = snapshot.StudentTasks.AsEnumerable();
        var notifications = snapshot.Notifications.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(filter.Semester))
        {
            var sem = filter.Semester.Trim();
            courses = courses.Where(c => string.Equals(c.Semester, sem, StringComparison.OrdinalIgnoreCase));
        }

        if (filter.CourseId.HasValue)
        {
            var cid = filter.CourseId.Value;
            courses = courses.Where(c => c.Id == cid);
        }

        var courseList = courses.ToList();
        var allowedCourseIds = courseList.Select(c => c.Id).ToHashSet();
        var hasCourseRestriction = !string.IsNullOrWhiteSpace(filter.Semester) || filter.CourseId.HasValue;

        if (hasCourseRestriction)
        {
            enrollments = enrollments.Where(e =>
                allowedCourseIds.Contains(e.CourseId) ||
                (!filter.CourseId.HasValue &&
                 !string.IsNullOrWhiteSpace(filter.Semester) &&
                 string.Equals(e.Semester, filter.Semester, StringComparison.OrdinalIgnoreCase)));
            grades = grades.Where(g => allowedCourseIds.Contains(g.CourseId));
            attendance = attendance.Where(a => allowedCourseIds.Contains(a.CourseId));
            tasks = tasks.Where(t => t.CourseId.HasValue && allowedCourseIds.Contains(t.CourseId.Value));
            notifications = notifications.Where(n => !n.RelatedCourseId.HasValue || allowedCourseIds.Contains(n.RelatedCourseId.Value));
        }

        var enrollmentList = enrollments.ToList();

        if (filter.StudentId.HasValue)
        {
            var sid = filter.StudentId.Value;
            students = students.Where(s => s.Id == sid);
            enrollmentList = enrollmentList.Where(e => e.StudentId == sid).ToList();
            grades = grades.Where(g => g.StudentId == sid);
            attendance = attendance.Where(a => a.StudentId == sid);
            tasks = tasks.Where(t => t.StudentId == sid);

            if (!access.IsAdmin && access.UserId.HasValue)
            {
                var uid = access.UserId.Value;
                notifications = notifications.Where(n =>
                    n.StudentId == sid ||
                    n.UserId == uid ||
                    (n.StudentId is null && n.UserId is null));
            }
            else
            {
                notifications = notifications.Where(n => n.StudentId == sid);
            }

            if (!hasCourseRestriction)
            {
                var studentCourseIds = enrollmentList
                    .Where(e => e.Status == EnrollmentStatus.Enrolled)
                    .Select(e => e.CourseId)
                    .Concat(grades.Select(g => g.CourseId))
                    .Concat(attendance.Select(a => a.CourseId))
                    .Concat(tasks.Where(t => t.CourseId.HasValue).Select(t => t.CourseId!.Value))
                    .ToHashSet();

                if (studentCourseIds.Count > 0)
                {
                    courseList = courseList.Where(c => studentCourseIds.Contains(c.Id)).ToList();
                }
            }
        }
        else if (hasCourseRestriction)
        {
            var enrolledStudentIds = enrollmentList.Select(e => e.StudentId)
                .Concat(grades.Select(g => g.StudentId))
                .Concat(attendance.Select(a => a.StudentId))
                .ToHashSet();

            students = students.Where(s => enrolledStudentIds.Contains(s.Id));
        }

        if (filter.DateFrom.HasValue)
        {
            var from = filter.DateFrom.Value;
            grades = grades.Where(g => DateOnly.FromDateTime(g.GradedAt) >= from);
            attendance = attendance.Where(a => a.Date >= from);
            tasks = tasks.Where(t => DateOnly.FromDateTime(t.DueDate) >= from);
            notifications = notifications.Where(n => DateOnly.FromDateTime(n.CreatedAt) >= from);
        }

        if (filter.DateTo.HasValue)
        {
            var to = filter.DateTo.Value;
            grades = grades.Where(g => DateOnly.FromDateTime(g.GradedAt) <= to);
            attendance = attendance.Where(a => a.Date <= to);
            tasks = tasks.Where(t => DateOnly.FromDateTime(t.DueDate) <= to);
            notifications = notifications.Where(n => DateOnly.FromDateTime(n.CreatedAt) <= to);
        }

        var activeTeacherIds = courseList
            .Where(c => c.TeacherId.HasValue)
            .Select(c => c.TeacherId!.Value)
            .ToHashSet();

        var teachers = hasCourseRestriction || filter.StudentId.HasValue
            ? snapshot.Teachers.Where(t => activeTeacherIds.Contains(t.Id)).ToList()
            : snapshot.Teachers.ToList();

        return new DashboardDatasetSnapshot(
            Students: students.ToList(),
            Teachers: teachers,
            Courses: courseList,
            Enrollments: enrollmentList,
            Grades: grades.ToList(),
            AttendanceRecords: attendance.ToList(),
            StudentTasks: tasks.ToList(),
            Notifications: notifications.ToList());
    }

    private AcademicAnalyticsDto BuildAcademicAnalytics(DashboardDatasetSnapshot data)
    {
        var maxGpa = _gpaCalculator.ScaleOptions.MaxGpaPoints;
        var utcNow = DateTime.UtcNow;

        var studentRankings = new List<StudentAcademicRankingDto>();

        foreach (var student in data.Students)
        {
            var studentGrades = data.Grades.Where(g => g.StudentId == student.Id).ToList();
            var gpaSummary = _gpaCalculator.CalculateStudentGpa(student, studentGrades);

            var enrolledCount = data.Enrollments.Count(e =>
                e.StudentId == student.Id && e.Status == EnrollmentStatus.Enrolled);

            var studentAtt = data.AttendanceRecords.Where(a => a.StudentId == student.Id).ToList();
            var attRate = CalculateAttendanceRate(studentAtt);

            var studentTasks = data.StudentTasks.Where(t => t.StudentId == student.Id).ToList();
            var completedTasks = studentTasks.Count(t => t.GetEffectiveStatus(utcNow) == StudentTaskStatus.Completed);
            var overdueTasks = studentTasks.Count(t => t.GetEffectiveStatus(utcNow) == StudentTaskStatus.Overdue);

            var (standingLabel, standingVariant) = ResolveAcademicStanding(
                gpaSummary.GradedCoursesCount,
                gpaSummary.CumulativeGpa);

            studentRankings.Add(new StudentAcademicRankingDto(
                StudentId: student.Id,
                StudentNumber: student.StudentNumber,
                StudentName: student.FullName,
                StudyProgram: student.StudyProgram,
                StudyYear: student.StudyYear,
                Status: student.Status,
                EnrolledCoursesCount: enrolledCount,
                GradedCoursesCount: gpaSummary.GradedCoursesCount,
                TotalCreditsEarned: gpaSummary.TotalCreditsEarned,
                CumulativeGpa: gpaSummary.CumulativeGpa,
                OverallAveragePercentage: gpaSummary.OverallAveragePercentage,
                StandingLabel: standingLabel,
                StandingVariant: standingVariant,
                AttendanceRatePercentage: attRate,
                CompletedTasksCount: completedTasks,
                OverdueTasksCount: overdueTasks));
        }

        var orderedStudents = studentRankings
            .OrderByDescending(s => s.GradedCoursesCount > 0)
            .ThenByDescending(s => s.CumulativeGpa)
            .ThenByDescending(s => s.OverallAveragePercentage)
            .ThenBy(s => s.StudentName)
            .ToList();

        var gradedStudents = orderedStudents.Where(s => s.GradedCoursesCount > 0).ToList();

        var averageGpa = gradedStudents.Count > 0
            ? Math.Round(gradedStudents.Average(s => s.CumulativeGpa), 2)
            : 0m;

        var medianGpa = CalculateMedian(gradedStudents.Select(s => s.CumulativeGpa).ToList());
        var highestGpa = gradedStudents.Count > 0 ? gradedStudents.Max(s => s.CumulativeGpa) : 0m;
        var lowestGpa = gradedStudents.Count > 0 ? gradedStudents.Min(s => s.CumulativeGpa) : 0m;

        var highestPerformer = gradedStudents.FirstOrDefault();
        var lowestPerformer = gradedStudents
            .OrderBy(s => s.CumulativeGpa)
            .ThenBy(s => s.OverallAveragePercentage)
            .ThenBy(s => s.StudentName)
            .FirstOrDefault();

        var overallAvgPercentage = data.Grades.Count > 0
            ? CalculateWeightedAveragePercentage(data.Grades)
            : 0m;

        var gpaDistribution = BuildGpaDistribution(gradedStudents);
        var gradeDistribution = BuildGradeDistribution(data.Grades);
        var coursePerformances = BuildCoursePerformanceAnalytics(data, utcNow);

        var gradedCourses = coursePerformances.Where(c => c.GradedAssessmentsCount > 0).ToList();
        var topCourse = gradedCourses
            .OrderByDescending(c => c.AverageScorePercentage)
            .ThenByDescending(c => c.PassRatePercentage)
            .FirstOrDefault();
        var lowestCourse = gradedCourses
            .OrderBy(c => c.AverageScorePercentage)
            .ThenBy(c => c.PassRatePercentage)
            .FirstOrDefault();

        var activeEnrollments = data.Enrollments.Where(e => e.Status == EnrollmentStatus.Enrolled).ToList();
        var totalCreditsEnrolled = activeEnrollments.Sum(e => e.Course?.Credits ?? 0);
        var totalCreditsEarned = gradedStudents.Sum(s => s.TotalCreditsEarned);
        var creditCompletionPct = totalCreditsEnrolled > 0
            ? Math.Round(Math.Min(100m, totalCreditsEarned * 100m / totalCreditsEnrolled), 1)
            : 0m;

        var passingAssessments = data.Grades.Count(g => _gpaCalculator.EvaluatePercentage(g.Score).GradePoints > 0m);
        var passingAssessmentsPct = data.Grades.Count > 0
            ? Math.Round(passingAssessments * 100m / data.Grades.Count, 1)
            : 0m;

        var progressOverview = new AcademicProgressOverviewDto(
            TotalEnrolledCourses: activeEnrollments.Count,
            GradedCoursesCount: gradedCourses.Count,
            TotalCreditsEnrolled: totalCreditsEnrolled,
            TotalCreditsEarned: totalCreditsEarned,
            CreditCompletionPercentage: creditCompletionPct,
            TotalAssessmentsRecorded: data.Grades.Count,
            PassingAssessmentsPercentage: passingAssessmentsPct,
            DeansListStudentsCount: gradedStudents.Count(s => s.CumulativeGpa >= 3.50m),
            AtRiskStudentsCount: gradedStudents.Count(s => s.CumulativeGpa < 2.00m));

        return new AcademicAnalyticsDto(
            AverageGpa: averageGpa,
            MedianGpa: medianGpa,
            HighestGpa: highestGpa,
            LowestGpa: lowestGpa,
            MaxGpaPoints: maxGpa,
            OverallAverageScorePercentage: overallAvgPercentage,
            TotalGradedStudents: gradedStudents.Count,
            TotalAssessmentsCount: data.Grades.Count,
            HighestPerformer: highestPerformer,
            LowestPerformer: lowestPerformer,
            TopPerformingCourse: topCourse,
            LowestPerformingCourse: lowestCourse,
            ProgressOverview: progressOverview,
            GpaDistribution: gpaDistribution,
            GradeDistribution: gradeDistribution,
            CoursePerformances: coursePerformances,
            StudentPerformances: orderedStudents);
    }

    private IReadOnlyList<GpaDistributionBucketDto> BuildGpaDistribution(
        IReadOnlyList<StudentAcademicRankingDto> gradedStudents)
    {
        var total = gradedStudents.Count;
        var definitions = new (string Label, string RangeDesc, decimal Min, decimal Max, string Variant)[]
        {
            ("3.50 – 4.00", "Dean's List / Honors", 3.50m, 4.00m, "success"),
            ("3.00 – 3.49", "Good Standing", 3.00m, 3.49m, "primary"),
            ("2.00 – 2.99", "Satisfactory", 2.00m, 2.99m, "info"),
            ("1.00 – 1.99", "Academic Warning", 1.00m, 1.99m, "warning"),
            ("0.00 – 0.99", "Academic Probation", 0.00m, 0.99m, "danger")
        };

        var result = new List<GpaDistributionBucketDto>(definitions.Length);
        foreach (var def in definitions)
        {
            var count = gradedStudents.Count(s => s.CumulativeGpa >= def.Min && s.CumulativeGpa <= def.Max);
            var pct = total > 0 ? Math.Round(count * 100m / total, 1) : 0m;
            result.Add(new GpaDistributionBucketDto(
                Label: def.Label,
                RangeDescription: def.RangeDesc,
                MinGpa: def.Min,
                MaxGpa: def.Max,
                StudentCount: count,
                PercentageOfStudents: pct,
                ColorVariant: def.Variant));
        }

        return result;
    }

    private IReadOnlyList<GradeDistributionBucketDto> BuildGradeDistribution(IReadOnlyList<Grade> grades)
    {
        var total = grades.Count;
        var evaluatedLetters = grades
            .Select(g => _gpaCalculator.EvaluatePercentage(g.Score).LetterGrade)
            .ToList();

        var bands = _gpaCalculator.ScaleOptions.Bands
            .OrderByDescending(b => b.MinPercentage)
            .ToList();

        var result = new List<GradeDistributionBucketDto>(bands.Count);
        foreach (var band in bands)
        {
            var count = evaluatedLetters.Count(l => string.Equals(l, band.LetterGrade, StringComparison.OrdinalIgnoreCase));
            var pct = total > 0 ? Math.Round(count * 100m / total, 1) : 0m;
            var variant = band.GradePoints switch
            {
                >= 3.5m => "success",
                >= 2.7m => "primary",
                >= 1.7m => "info",
                >= 1.0m => "warning",
                _ => "danger"
            };

            result.Add(new GradeDistributionBucketDto(
                LetterGrade: band.LetterGrade,
                Description: band.Description,
                GradePoints: band.GradePoints,
                AssessmentCount: count,
                PercentageOfAssessments: pct,
                ColorVariant: variant));
        }

        return result;
    }

    private IReadOnlyList<CoursePerformanceAnalyticsDto> BuildCoursePerformanceAnalytics(
        DashboardDatasetSnapshot data,
        DateTime utcNow)
    {
        var list = new List<CoursePerformanceAnalyticsDto>(data.Courses.Count);

        foreach (var course in data.Courses.OrderBy(c => c.CourseCode))
        {
            var enrolledCount = data.Enrollments.Count(e =>
                e.CourseId == course.Id && e.Status == EnrollmentStatus.Enrolled);

            var courseGrades = data.Grades.Where(g => g.CourseId == course.Id).ToList();
            var avgPct = CalculateWeightedAveragePercentage(courseGrades);
            var (letter, points) = courseGrades.Count > 0
                ? _gpaCalculator.EvaluatePercentage(avgPct)
                : ("N/A", 0m);

            var passingCount = courseGrades.Count(g => _gpaCalculator.EvaluatePercentage(g.Score).GradePoints > 0m);
            var passRate = courseGrades.Count > 0
                ? Math.Round(passingCount * 100m / courseGrades.Count, 1)
                : 0m;

            var courseAtt = data.AttendanceRecords.Where(a => a.CourseId == course.Id).ToList();
            var attRate = CalculateAttendanceRate(courseAtt);

            var courseTasks = data.StudentTasks.Where(t => t.CourseId == course.Id).ToList();
            var completedTasks = courseTasks.Count(t => t.GetEffectiveStatus(utcNow) == StudentTaskStatus.Completed);

            list.Add(new CoursePerformanceAnalyticsDto(
                CourseId: course.Id,
                CourseCode: course.CourseCode,
                CourseTitle: course.Title,
                Department: course.Department,
                Semester: course.Semester,
                Credits: course.Credits,
                TeacherName: course.Teacher?.FullName,
                IsActive: course.IsActive,
                EnrolledStudentsCount: enrolledCount,
                GradedAssessmentsCount: courseGrades.Count,
                AverageScorePercentage: avgPct,
                AverageGradePoints: points,
                LetterGrade: letter,
                PassRatePercentage: passRate,
                AttendanceRatePercentage: attRate,
                TotalTasksCount: courseTasks.Count,
                CompletedTasksCount: completedTasks));
        }

        return list;
    }

    private static AttendanceAnalyticsDto BuildAttendanceAnalytics(DashboardDatasetSnapshot data)
    {
        var records = data.AttendanceRecords;
        var total = records.Count;
        var present = records.Count(r => r.Status == AttendanceStatus.Present);
        var late = records.Count(r => r.Status == AttendanceStatus.Late);
        var absent = records.Count(r => r.Status == AttendanceStatus.Absent);

        var overallRate = CalculateAttendanceRate(records);
        var strictPresentRate = total > 0 ? Math.Round(present * 100m / total, 1) : 0m;

        var statusDistribution = new List<AttendanceStatusSliceDto>
        {
            new(AttendanceStatus.Present, "Present", present, total > 0 ? Math.Round(present * 100m / total, 1) : 0m, "success"),
            new(AttendanceStatus.Late, "Late", late, total > 0 ? Math.Round(late * 100m / total, 1) : 0m, "warning"),
            new(AttendanceStatus.Absent, "Absent", absent, total > 0 ? Math.Round(absent * 100m / total, 1) : 0m, "danger")
        };

        var courseStats = data.Courses
            .OrderBy(c => c.CourseCode)
            .Select(c =>
            {
                var cRecs = records.Where(r => r.CourseId == c.Id).ToList();
                var cTot = cRecs.Count;
                var cPres = cRecs.Count(r => r.Status == AttendanceStatus.Present);
                var cLate = cRecs.Count(r => r.Status == AttendanceStatus.Late);
                var cAbs = cRecs.Count(r => r.Status == AttendanceStatus.Absent);

                return new CourseAttendanceAnalyticsDto(
                    CourseId: c.Id,
                    CourseCode: c.CourseCode,
                    CourseTitle: c.Title,
                    Semester: c.Semester,
                    TeacherName: c.Teacher?.FullName,
                    TotalSessions: cTot,
                    PresentCount: cPres,
                    LateCount: cLate,
                    AbsentCount: cAbs,
                    AttendanceRatePercentage: CalculateAttendanceRate(cRecs),
                    StrictPresentPercentage: cTot > 0 ? Math.Round(cPres * 100m / cTot, 1) : 0m);
            })
            .ToList();

        var studentStats = data.Students
            .Select(s =>
            {
                var sRecs = records.Where(r => r.StudentId == s.Id).ToList();
                var sTot = sRecs.Count;
                var sPres = sRecs.Count(r => r.Status == AttendanceStatus.Present);
                var sLate = sRecs.Count(r => r.Status == AttendanceStatus.Late);
                var sAbs = sRecs.Count(r => r.Status == AttendanceStatus.Absent);
                var rate = CalculateAttendanceRate(sRecs);
                var strict = sTot > 0 ? Math.Round(sPres * 100m / sTot, 1) : 0m;
                var atRisk = sTot > 0 && rate < 75.0m;

                return new StudentAttendanceAnalyticsDto(
                    StudentId: s.Id,
                    StudentNumber: s.StudentNumber,
                    StudentName: s.FullName,
                    StudyProgram: s.StudyProgram,
                    StudyYear: s.StudyYear,
                    TotalSessions: sTot,
                    PresentCount: sPres,
                    LateCount: sLate,
                    AbsentCount: sAbs,
                    AttendanceRatePercentage: rate,
                    StrictPresentPercentage: strict,
                    IsAtRisk: atRisk);
            })
            .OrderByDescending(s => s.TotalSessions > 0)
            .ThenByDescending(s => s.AttendanceRatePercentage)
            .ThenBy(s => s.StudentName)
            .ToList();

        var distinctDates = records.Select(r => r.Date).Distinct().OrderBy(d => d).ToList();
        var trends = new List<AttendanceTrendPointDto>();

        if (distinctDates.Count <= 12)
        {
            foreach (var date in distinctDates)
            {
                var dRecs = records.Where(r => r.Date == date).ToList();
                var dPres = dRecs.Count(r => r.Status == AttendanceStatus.Present);
                var dLate = dRecs.Count(r => r.Status == AttendanceStatus.Late);
                var dAbs = dRecs.Count(r => r.Status == AttendanceStatus.Absent);

                trends.Add(new AttendanceTrendPointDto(
                    PeriodStartDate: date,
                    PeriodLabel: date.ToString("MMM dd"),
                    TotalSessions: dRecs.Count,
                    PresentCount: dPres,
                    LateCount: dLate,
                    AbsentCount: dAbs,
                    AttendanceRatePercentage: CalculateAttendanceRate(dRecs)));
            }
        }
        else
        {
            var groupedByWeek = records
                .GroupBy(r =>
                {
                    var diff = ((int)r.Date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
                    return r.Date.AddDays(-diff);
                })
                .OrderBy(g => g.Key);

            foreach (var grp in groupedByWeek)
            {
                var gRecs = grp.ToList();
                var gPres = gRecs.Count(r => r.Status == AttendanceStatus.Present);
                var gLate = gRecs.Count(r => r.Status == AttendanceStatus.Late);
                var gAbs = gRecs.Count(r => r.Status == AttendanceStatus.Absent);

                trends.Add(new AttendanceTrendPointDto(
                    PeriodStartDate: grp.Key,
                    PeriodLabel: $"Wk {grp.Key:MMM dd}",
                    TotalSessions: gRecs.Count,
                    PresentCount: gPres,
                    LateCount: gLate,
                    AbsentCount: gAbs,
                    AttendanceRatePercentage: CalculateAttendanceRate(gRecs)));
            }
        }

        return new AttendanceAnalyticsDto(
            TotalSessions: total,
            PresentCount: present,
            LateCount: late,
            AbsentCount: absent,
            OverallAttendancePercentage: overallRate,
            StrictPresentPercentage: strictPresentRate,
            AtRiskStudentsCount: studentStats.Count(s => s.IsAtRisk),
            StatusDistribution: statusDistribution,
            CourseStatistics: courseStats,
            StudentStatistics: studentStats,
            AttendanceTrends: trends);
    }

    private static TaskProductivityAnalyticsDto BuildTaskProductivityAnalytics(
        DashboardDatasetSnapshot data,
        DateTime utcNow)
    {
        var tasks = data.StudentTasks;
        var total = tasks.Count;

        var evaluated = tasks
            .Select(t => (Task: t, EffectiveStatus: t.GetEffectiveStatus(utcNow)))
            .ToList();

        var completed = evaluated.Count(x => x.EffectiveStatus == StudentTaskStatus.Completed);
        var pending = evaluated.Count(x => x.EffectiveStatus == StudentTaskStatus.Pending);
        var inProgress = evaluated.Count(x => x.EffectiveStatus == StudentTaskStatus.InProgress);
        var overdue = evaluated.Count(x => x.EffectiveStatus == StudentTaskStatus.Overdue);
        var active = pending + inProgress + overdue;

        var dueSoon = evaluated.Count(x =>
            x.EffectiveStatus != StudentTaskStatus.Completed &&
            x.EffectiveStatus != StudentTaskStatus.Overdue &&
            x.Task.DueDate >= utcNow &&
            x.Task.DueDate <= utcNow.AddHours(48));

        var completionRate = total > 0 ? Math.Round(completed * 100m / total, 1) : 0m;

        var completedItems = evaluated.Where(x => x.EffectiveStatus == StudentTaskStatus.Completed).ToList();
        var onTimeCompleted = completedItems.Count(x =>
            !x.Task.CompletedAt.HasValue || x.Task.CompletedAt.Value <= x.Task.DueDate);
        var onTimeRate = completedItems.Count > 0
            ? Math.Round(onTimeCompleted * 100m / completedItems.Count, 1)
            : 0m;

        var avgPerStudent = data.Students.Count > 0
            ? Math.Round((decimal)total / data.Students.Count, 1)
            : 0m;

        var priorityDefs = new (TaskPriority Priority, string Label, string Variant)[]
        {
            (TaskPriority.High, "High Priority", "danger"),
            (TaskPriority.Medium, "Medium Priority", "warning"),
            (TaskPriority.Low, "Low Priority", "info")
        };

        var priorityBreakdown = priorityDefs
            .Select(def =>
            {
                var pItems = evaluated.Where(x => x.Task.Priority == def.Priority).ToList();
                var pTotal = pItems.Count;
                var pComp = pItems.Count(x => x.EffectiveStatus == StudentTaskStatus.Completed);
                var pPend = pItems.Count(x => x.EffectiveStatus == StudentTaskStatus.Pending);
                var pProg = pItems.Count(x => x.EffectiveStatus == StudentTaskStatus.InProgress);
                var pOver = pItems.Count(x => x.EffectiveStatus == StudentTaskStatus.Overdue);

                return new TaskPriorityAnalyticsDto(
                    Priority: def.Priority,
                    Label: def.Label,
                    TotalCount: pTotal,
                    PendingCount: pPend,
                    InProgressCount: pProg,
                    CompletedCount: pComp,
                    OverdueCount: pOver,
                    PercentageOfTotal: total > 0 ? Math.Round(pTotal * 100m / total, 1) : 0m,
                    CompletionRatePercentage: pTotal > 0 ? Math.Round(pComp * 100m / pTotal, 1) : 0m,
                    ColorVariant: def.Variant);
            })
            .ToList();

        var statusBreakdown = new List<TaskStatusAnalyticsDto>
        {
            new(StudentTaskStatus.Completed, "Completed", completed, total > 0 ? Math.Round(completed * 100m / total, 1) : 0m, "success"),
            new(StudentTaskStatus.InProgress, "In Progress", inProgress, total > 0 ? Math.Round(inProgress * 100m / total, 1) : 0m, "primary"),
            new(StudentTaskStatus.Pending, "Pending", pending, total > 0 ? Math.Round(pending * 100m / total, 1) : 0m, "secondary"),
            new(StudentTaskStatus.Overdue, "Overdue", overdue, total > 0 ? Math.Round(overdue * 100m / total, 1) : 0m, "danger")
        };

        var courseBreakdown = evaluated
            .GroupBy(x => new
            {
                x.Task.CourseId,
                CourseCode = x.Task.Course?.CourseCode ?? "GENERAL",
                CourseTitle = x.Task.Course?.Title ?? "General Academic Tasks"
            })
            .OrderBy(g => g.Key.CourseCode)
            .Select(g =>
            {
                var gTot = g.Count();
                var gComp = g.Count(x => x.EffectiveStatus == StudentTaskStatus.Completed);
                var gPend = g.Count(x => x.EffectiveStatus == StudentTaskStatus.Pending);
                var gProg = g.Count(x => x.EffectiveStatus == StudentTaskStatus.InProgress);
                var gOver = g.Count(x => x.EffectiveStatus == StudentTaskStatus.Overdue);

                return new CourseTaskAnalyticsDto(
                    CourseId: g.Key.CourseId,
                    CourseCode: g.Key.CourseCode,
                    CourseTitle: g.Key.CourseTitle,
                    TotalTasks: gTot,
                    CompletedTasks: gComp,
                    PendingTasks: gPend,
                    InProgressTasks: gProg,
                    OverdueTasks: gOver,
                    CompletionRatePercentage: gTot > 0 ? Math.Round(gComp * 100m / gTot, 1) : 0m);
            })
            .ToList();

        var upcomingDeadlines = evaluated
            .Where(x => x.EffectiveStatus != StudentTaskStatus.Completed && x.Task.DueDate >= utcNow)
            .OrderBy(x => x.Task.DueDate)
            .ThenByDescending(x => x.Task.Priority)
            .Take(8)
            .Select(x => MapTaskDto(x.Task, utcNow))
            .ToList();

        var overdueItems = evaluated
            .Where(x => x.EffectiveStatus == StudentTaskStatus.Overdue)
            .OrderBy(x => x.Task.DueDate)
            .ThenByDescending(x => x.Task.Priority)
            .Select(x => MapTaskDto(x.Task, utcNow))
            .ToList();

        return new TaskProductivityAnalyticsDto(
            TotalTasks: total,
            CompletedTasks: completed,
            PendingTasks: pending,
            InProgressTasks: inProgress,
            ActiveTasks: active,
            OverdueTasks: overdue,
            DueSoonTasks: dueSoon,
            CompletionRatePercentage: completionRate,
            OnTimeCompletionRatePercentage: onTimeRate,
            AverageTasksPerStudent: avgPerStudent,
            PriorityBreakdown: priorityBreakdown,
            StatusBreakdown: statusBreakdown,
            CourseBreakdown: courseBreakdown,
            UpcomingDeadlines: upcomingDeadlines,
            OverdueTaskItems: overdueItems);
    }

    private IReadOnlyList<RecentAcademicActivityDto> BuildRecentActivityFeed(
        DashboardDatasetSnapshot data,
        DateTime utcNow)
    {
        var feed = new List<RecentAcademicActivityDto>();

        foreach (var grade in data.Grades)
        {
            var (letter, _) = _gpaCalculator.EvaluatePercentage(grade.Score);
            feed.Add(new RecentAcademicActivityDto(
                Category: "Grade",
                Title: $"{grade.Course?.CourseCode ?? "Course"}: {grade.AssessmentTitle}",
                Subtitle: $"{grade.Student?.FullName ?? "Student"} ({grade.AssessmentType})",
                BadgeText: $"{grade.Score:F1}% ({letter})",
                BadgeVariant: grade.Score >= 80m ? "success" : grade.Score >= 60m ? "primary" : "danger",
                OccurredAt: grade.GradedAt,
                StudentId: grade.StudentId,
                StudentName: grade.Student?.FullName,
                CourseId: grade.CourseId,
                CourseCode: grade.Course?.CourseCode));
        }

        foreach (var task in data.StudentTasks)
        {
            var eff = task.GetEffectiveStatus(utcNow);
            var occurred = task.CompletedAt ?? task.UpdatedAt;
            var (badge, variant) = eff switch
            {
                StudentTaskStatus.Completed => ("Completed", "success"),
                StudentTaskStatus.Overdue => ("Overdue", "danger"),
                StudentTaskStatus.InProgress => ("In Progress", "primary"),
                _ => ("Pending", "warning")
            };

            feed.Add(new RecentAcademicActivityDto(
                Category: "Task",
                Title: task.Title,
                Subtitle: $"{task.Student?.FullName ?? "Student"} • Due {task.DueDate:MMM dd, yyyy}",
                BadgeText: badge,
                BadgeVariant: variant,
                OccurredAt: occurred,
                StudentId: task.StudentId,
                StudentName: task.Student?.FullName,
                CourseId: task.CourseId,
                CourseCode: task.Course?.CourseCode));
        }

        foreach (var att in data.AttendanceRecords)
        {
            var variant = att.Status switch
            {
                AttendanceStatus.Present => "success",
                AttendanceStatus.Late => "warning",
                _ => "danger"
            };

            feed.Add(new RecentAcademicActivityDto(
                Category: "Attendance",
                Title: $"{att.Course?.CourseCode ?? "Course"}: {att.Topic ?? "Class Session"}",
                Subtitle: $"{att.Student?.FullName ?? "Student"} • {att.Date:MMM dd, yyyy}",
                BadgeText: att.Status.ToString(),
                BadgeVariant: variant,
                OccurredAt: att.RecordedAt,
                StudentId: att.StudentId,
                StudentName: att.Student?.FullName,
                CourseId: att.CourseId,
                CourseCode: att.Course?.CourseCode));
        }

        return feed
            .OrderByDescending(a => a.OccurredAt)
            .Take(10)
            .ToList();
    }

    private static decimal CalculateWeightedAveragePercentage(IReadOnlyList<Grade> grades)
    {
        if (grades.Count == 0)
        {
            return 0m;
        }

        var totalWeight = grades.Sum(g => g.Weight > 0 ? g.Weight : 1m);
        if (totalWeight <= 0m)
        {
            return Math.Round(grades.Average(g => g.Score), 1);
        }

        var weightedSum = grades.Sum(g => g.Score * (g.Weight > 0 ? g.Weight : 1m));
        return Math.Round(weightedSum / totalWeight, 1);
    }

    private static decimal CalculateAttendanceRate(IReadOnlyList<AttendanceRecord> records)
    {
        if (records.Count == 0)
        {
            return 0m;
        }

        var present = records.Count(r => r.Status == AttendanceStatus.Present);
        var late = records.Count(r => r.Status == AttendanceStatus.Late);
        return Math.Round((present + late * 0.5m) * 100m / records.Count, 1);
    }

    private static decimal CalculateMedian(IReadOnlyList<decimal> values)
    {
        if (values.Count == 0)
        {
            return 0m;
        }

        var sorted = values.OrderBy(v => v).ToList();
        var mid = sorted.Count / 2;
        if (sorted.Count % 2 == 1)
        {
            return Math.Round(sorted[mid], 2);
        }

        return Math.Round((sorted[mid - 1] + sorted[mid]) / 2m, 2);
    }

    private static (string Label, string Variant) ResolveAcademicStanding(int gradedCoursesCount, decimal gpa)
    {
        if (gradedCoursesCount == 0)
        {
            return ("No Grades Yet", "secondary");
        }

        return gpa switch
        {
            >= 3.50m => ("Dean's List", "success"),
            >= 3.00m => ("Good Standing", "primary"),
            >= 2.00m => ("Satisfactory", "info"),
            >= 1.00m => ("Academic Warning", "warning"),
            _ => ("At Risk", "danger")
        };
    }

    private static StudentTaskDto MapTaskDto(StudentTask task, DateTime utcNow)
    {
        var effectiveStatus = task.GetEffectiveStatus(utcNow);
        var isOverdue = effectiveStatus == StudentTaskStatus.Overdue;
        var isDueSoon = !isOverdue &&
                        effectiveStatus != StudentTaskStatus.Completed &&
                        task.DueDate >= utcNow &&
                        task.DueDate <= utcNow.AddHours(48);

        return new StudentTaskDto(
            Id: task.Id,
            StudentId: task.StudentId,
            StudentName: task.Student?.FullName ?? $"Student #{task.StudentId}",
            StudentNumber: task.Student?.StudentNumber ?? string.Empty,
            CourseId: task.CourseId,
            CourseCode: task.Course?.CourseCode,
            CourseTitle: task.Course?.Title,
            Title: task.Title,
            Description: task.Description,
            DueDate: task.DueDate,
            Priority: task.Priority,
            Status: effectiveStatus,
            IsOverdue: isOverdue,
            IsDueSoon: isDueSoon,
            CreatedAt: task.CreatedAt,
            UpdatedAt: task.UpdatedAt,
            CompletedAt: task.CompletedAt);
    }

    private static NotificationDto MapNotificationDto(Notification n) =>
        new(
            Id: n.Id,
            UserId: n.UserId,
            StudentId: n.StudentId,
            StudentName: n.Student?.FullName,
            RelatedTaskId: n.RelatedTaskId,
            RelatedTaskTitle: n.RelatedTask?.Title,
            RelatedCourseId: n.RelatedCourseId,
            RelatedCourseCode: n.RelatedCourse?.CourseCode,
            Title: n.Title,
            Message: n.Message,
            Type: n.Type,
            IsRead: n.IsRead,
            CreatedAt: n.CreatedAt,
            ReadAt: n.ReadAt);

    private static ServiceResult<T> PropagateFailure<T>(ServiceResult<DashboardFilterDto> failed) =>
        ServiceResult<T>.Failure(failed.Errors, failed.ErrorType);
}
