using CRM_System.Core.Academic.Models;
using CRM_System.Core.Academic.Services;
using CRM_System.Core.Students.Models;

namespace CRM_System.Tests.Academic;

public class GpaCalculatorTests
{
    [Fact]
    public void EvaluatePercentage_DefaultStandardScale_MapsScoresToExpectedLettersAndPoints()
    {
        var calculator = new ConfigurableGpaCalculator();

        Assert.Equal(("A", 4.0m), calculator.EvaluatePercentage(95.0m));
        Assert.Equal(("A", 4.0m), calculator.EvaluatePercentage(90.0m));
        Assert.Equal(("B", 3.0m), calculator.EvaluatePercentage(85.5m));
        Assert.Equal(("C", 2.0m), calculator.EvaluatePercentage(74.0m));
        Assert.Equal(("D", 1.0m), calculator.EvaluatePercentage(61.0m));
        Assert.Equal(("F", 0.0m), calculator.EvaluatePercentage(42.0m));
    }

    [Fact]
    public void EvaluatePercentage_CustomConfiguredGradingScale_UsesConfiguredBands()
    {
        var customScale = new GradingScaleOptions
        {
            ScaleName = "University Plus/Minus 4.3 Scale",
            MaxGpaPoints = 4.3m,
            Bands =
            [
                new() { LetterGrade = "A+", MinPercentage = 97m, MaxPercentage = 100m, GradePoints = 4.3m, Description = "Distinction" },
                new() { LetterGrade = "A", MinPercentage = 90m, MaxPercentage = 96.99m, GradePoints = 4.0m, Description = "Excellent" },
                new() { LetterGrade = "B+", MinPercentage = 85m, MaxPercentage = 89.99m, GradePoints = 3.3m, Description = "Very Good" },
                new() { LetterGrade = "Pass", MinPercentage = 50m, MaxPercentage = 84.99m, GradePoints = 2.0m, Description = "Pass" },
                new() { LetterGrade = "Fail", MinPercentage = 0m, MaxPercentage = 49.99m, GradePoints = 0.0m, Description = "Fail" }
            ]
        };

        var calculator = new ConfigurableGpaCalculator(customScale);

        Assert.Equal(("A+", 4.3m), calculator.EvaluatePercentage(98.5m));
        Assert.Equal(("B+", 3.3m), calculator.EvaluatePercentage(87.0m));
        Assert.Equal(("Pass", 2.0m), calculator.EvaluatePercentage(55.0m));
        Assert.Equal(("Fail", 0.0m), calculator.EvaluatePercentage(30.0m));
    }

    [Fact]
    public void CalculateStudentGpa_EmptyGrades_ReturnsZeroGpaSafely()
    {
        var calculator = new ConfigurableGpaCalculator();
        var student = new Student
        {
            Id = 1,
            FullName = "Aidar Nurlanov",
            StudentNumber = "STU-2026-0001"
        };

        var summary = calculator.CalculateStudentGpa(student, []);

        Assert.Equal(0.00m, summary.CumulativeGpa);
        Assert.Equal(0.00m, summary.OverallAveragePercentage);
        Assert.Equal(0, summary.GradedCoursesCount);
        Assert.Equal(0, summary.TotalCreditsEarned);
        Assert.Empty(summary.CourseSummaries);
    }

    [Fact]
    public void CalculateStudentGpa_CreditWeightedCoursesAndWeightedAssessments_ComputesAccurateGpa()
    {
        var calculator = new ConfigurableGpaCalculator();
        var student = new Student
        {
            Id = 1,
            FullName = "Aidar Nurlanov",
            StudentNumber = "STU-2026-0001"
        };

        var course1 = new Course { Id = 10, CourseCode = "CS-301", Title = "C# Architecture", Credits = 6 };
        var course2 = new Course { Id = 20, CourseCode = "CS-302", Title = "Database Engineering", Credits = 4 };

        // Course 1 (6 credits): Midterm 90 (0.4) + Final 95 (0.6) = 36 + 57 = 93.0% -> A (4.0 points)
        // Course 2 (4 credits): Midterm 80 (0.5) + Final 86 (0.5) = 40 + 43 = 83.0% -> B (3.0 points)
        // Expected Credit-Weighted GPA = (4.0 * 6 + 3.0 * 4) / (6 + 4) = (24 + 12) / 10 = 3.60
        var grades = new List<Grade>
        {
            new() { Id = 1, StudentId = 1, CourseId = 10, Course = course1, AssessmentTitle = "Midterm", AssessmentType = "Midterm", Score = 90m, Weight = 0.4m },
            new() { Id = 2, StudentId = 1, CourseId = 10, Course = course1, AssessmentTitle = "Final", AssessmentType = "Final", Score = 95m, Weight = 0.6m },
            new() { Id = 3, StudentId = 1, CourseId = 20, Course = course2, AssessmentTitle = "Midterm", AssessmentType = "Midterm", Score = 80m, Weight = 0.5m },
            new() { Id = 4, StudentId = 1, CourseId = 20, Course = course2, AssessmentTitle = "Final", AssessmentType = "Final", Score = 86m, Weight = 0.5m }
        };

        var summary = calculator.CalculateStudentGpa(student, grades);

        Assert.Equal(3.60m, summary.CumulativeGpa);
        Assert.Equal(2, summary.GradedCoursesCount);
        Assert.Equal(10, summary.TotalCreditsEarned);
        Assert.Equal(88.00m, summary.OverallAveragePercentage);
        Assert.Equal("A", summary.CourseSummaries[0].LetterGrade);
        Assert.Equal(93.00m, summary.CourseSummaries[0].AveragePercentage);
        Assert.Equal("B", summary.CourseSummaries[1].LetterGrade);
        Assert.Equal(83.00m, summary.CourseSummaries[1].AveragePercentage);
    }
}
