using CRM_System.Core.Academic.DTOs;
using CRM_System.Core.Academic.Interfaces;
using CRM_System.Core.Academic.Models;
using CRM_System.Core.Students.Models;

namespace CRM_System.Core.Academic.Services;

public class ConfigurableGpaCalculator : IGpaCalculator
{
    public GradingScaleOptions ScaleOptions { get; }

    public ConfigurableGpaCalculator(GradingScaleOptions? options = null)
    {
        ScaleOptions = options is { Bands.Count: > 0 }
            ? options
            : new GradingScaleOptions();
    }

    public (string LetterGrade, decimal GradePoints) EvaluatePercentage(decimal percentage)
    {
        var clamped = Math.Clamp(Math.Round(percentage, 2), 0m, 100m);
        var orderedBands = ScaleOptions.Bands
            .OrderByDescending(b => b.MinPercentage)
            .ToList();

        foreach (var band in orderedBands)
        {
            if (clamped >= band.MinPercentage)
            {
                return (band.LetterGrade, band.GradePoints);
            }
        }

        var fallback = orderedBands.LastOrDefault();
        return fallback is not null
            ? (fallback.LetterGrade, fallback.GradePoints)
            : ("F", 0.0m);
    }

    public StudentGpaSummaryDto CalculateStudentGpa(Student student, IReadOnlyList<Grade> grades)
    {
        var orderedBands = ScaleOptions.Bands
            .OrderByDescending(b => b.MinPercentage)
            .ToList();

        if (grades.Count == 0)
        {
            return new StudentGpaSummaryDto(
                StudentId: student.Id,
                StudentName: student.FullName,
                StudentNumber: student.StudentNumber,
                ScaleName: ScaleOptions.ScaleName,
                MaxGpaPoints: ScaleOptions.MaxGpaPoints,
                CumulativeGpa: 0.00m,
                OverallAveragePercentage: 0.00m,
                GradedCoursesCount: 0,
                TotalCreditsEarned: 0,
                CourseSummaries: [],
                ScaleBands: orderedBands);
        }

        var courseGroups = grades
            .GroupBy(g => g.CourseId)
            .ToList();

        var courseSummaries = new List<CourseGradeSummaryDto>();
        decimal totalWeightedGradePoints = 0m;
        int totalGradedCredits = 0;
        int totalCreditsEarned = 0;

        foreach (var group in courseGroups)
        {
            var first = group.First();
            var courseCode = first.Course?.CourseCode ?? $"CRS-{group.Key}";
            var courseTitle = first.Course?.Title ?? $"Course #{group.Key}";
            var credits = Math.Max(1, first.Course?.Credits ?? 3);

            var totalWeight = group.Sum(g => g.Weight <= 0m ? 1.0m : g.Weight);
            var weightedSum = group.Sum(g => g.Score * (g.Weight <= 0m ? 1.0m : g.Weight));
            var courseAvg = totalWeight > 0m
                ? Math.Round(weightedSum / totalWeight, 2)
                : Math.Round(group.Average(g => g.Score), 2);

            var (letter, points) = EvaluatePercentage(courseAvg);

            var assessmentDtos = group
                .OrderByDescending(g => g.GradedAt)
                .Select(g =>
                {
                    var (gLetter, gPoints) = EvaluatePercentage(g.Score);
                    return new GradeDto(
                        g.Id,
                        student.Id,
                        student.FullName,
                        student.StudentNumber,
                        g.CourseId,
                        courseCode,
                        courseTitle,
                        credits,
                        g.AssessmentTitle,
                        g.AssessmentType,
                        Math.Round(g.Score, 2),
                        g.Weight,
                        gLetter,
                        gPoints,
                        g.Feedback,
                        g.GradedAt);
                })
                .ToList();

            courseSummaries.Add(new CourseGradeSummaryDto(
                CourseId: group.Key,
                CourseCode: courseCode,
                CourseTitle: courseTitle,
                Credits: credits,
                AssessmentsCount: assessmentDtos.Count,
                AveragePercentage: courseAvg,
                LetterGrade: letter,
                GradePoints: points,
                Assessments: assessmentDtos));

            totalWeightedGradePoints += points * credits;
            totalGradedCredits += credits;
            if (points > 0m)
            {
                totalCreditsEarned += credits;
            }
        }

        courseSummaries = courseSummaries
            .OrderBy(c => c.CourseCode)
            .ToList();

        var cumulativeGpa = totalGradedCredits > 0
            ? Math.Round(totalWeightedGradePoints / totalGradedCredits, 2)
            : 0.00m;

        var overallAveragePercentage = courseSummaries.Count > 0
            ? Math.Round(courseSummaries.Average(c => c.AveragePercentage), 2)
            : 0.00m;

        return new StudentGpaSummaryDto(
            StudentId: student.Id,
            StudentName: student.FullName,
            StudentNumber: student.StudentNumber,
            ScaleName: ScaleOptions.ScaleName,
            MaxGpaPoints: ScaleOptions.MaxGpaPoints,
            CumulativeGpa: cumulativeGpa,
            OverallAveragePercentage: overallAveragePercentage,
            GradedCoursesCount: courseSummaries.Count,
            TotalCreditsEarned: totalCreditsEarned,
            CourseSummaries: courseSummaries,
            ScaleBands: orderedBands);
    }
}
