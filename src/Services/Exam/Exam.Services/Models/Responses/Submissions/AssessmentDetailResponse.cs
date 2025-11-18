using Exam.Domain.Enums;
using Exam.Services.Models.ScoreStructureJson.Assessments;
using Exam.Services.Models.ScoreStructureJson.ExamSubjects;

namespace Exam.Services.Models.Responses.Submissions;

public class AssessmentDetailResponse
{
    public Guid Id { get; set; }
    public Guid SubmissionId { get; set; }
    public string SubmissionName { get; set; } = null!;
    public AssessmentStatus Status { get; set; }
    public decimal? Score { get; set; }
    public string? Comment { get; set; }
    // Rubric từ ExamSubject
    public ScoreStructure ScoreStructure { get; set; } = null!;
    // Kết quả chấm (nếu đã chấm trước đó)
    public ScoreDetail? ScoreDetail { get; set; }
}
