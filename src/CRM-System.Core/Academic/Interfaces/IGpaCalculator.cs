using CRM_System.Core.Academic.DTOs;
using CRM_System.Core.Academic.Models;
using CRM_System.Core.Students.Models;

namespace CRM_System.Core.Academic.Interfaces;

public interface IGpaCalculator
{
    GradingScaleOptions ScaleOptions { get; }
    (string LetterGrade, decimal GradePoints) EvaluatePercentage(decimal percentage);
    StudentGpaSummaryDto CalculateStudentGpa(Student student, IReadOnlyList<Grade> grades);
}
