using Exam.Services.Models.ScoreStructureJson.Assessments;

namespace Exam.Services.Models.Requests.Assessments;

public class GradeAssessmentRequest
{
    public ScoreDetail ScoreDetail { get; set; } = null!;
    public string? Comment { get; set; }
}
