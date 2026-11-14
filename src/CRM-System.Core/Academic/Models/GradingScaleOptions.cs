namespace CRM_System.Core.Academic.Models;

public class GradingScaleOptions
{
    public const string SectionName = "GradingScale";

    public string ScaleName { get; set; } = "Standard 4.0 Demonstration Scale";
    public decimal MaxGpaPoints { get; set; } = 4.0m;
    public List<GradingScaleBand> Bands { get; set; } = GetDefaultBands();

    public static List<GradingScaleBand> GetDefaultBands() =>
    [
        new() { LetterGrade = "A", MinPercentage = 90.0m, MaxPercentage = 100.0m, GradePoints = 4.0m, Description = "Excellent" },
        new() { LetterGrade = "B", MinPercentage = 80.0m, MaxPercentage = 89.99m, GradePoints = 3.0m, Description = "Good" },
        new() { LetterGrade = "C", MinPercentage = 70.0m, MaxPercentage = 79.99m, GradePoints = 2.0m, Description = "Satisfactory" },
        new() { LetterGrade = "D", MinPercentage = 60.0m, MaxPercentage = 69.99m, GradePoints = 1.0m, Description = "Passing" },
        new() { LetterGrade = "F", MinPercentage = 0.0m, MaxPercentage = 59.99m, GradePoints = 0.0m, Description = "Failing" }
    ];
}

public class GradingScaleBand
{
    public string LetterGrade { get; set; } = string.Empty;
    public decimal MinPercentage { get; set; }
    public decimal MaxPercentage { get; set; }
    public decimal GradePoints { get; set; }
    public string Description { get; set; } = string.Empty;
}
