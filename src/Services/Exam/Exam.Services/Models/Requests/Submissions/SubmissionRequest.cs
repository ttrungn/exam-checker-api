using Exam.Domain.Enums;
using Exam.Services.Features.Submissions.Queries.GetSubmissionByUser;

namespace Exam.Services.Models.Requests.Submissions;

public class SubmissionRequest
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int IndexFrom { get; set; } = 1;
    public string? ExamCode { get; init; }
    public string? SubjectCode { get; init; }
    public SubmissionStatus? Status { get; init; }
    public string? SubmissionName { get; init; }
    public AssessmentStatus? AssessmentStatus { get; init; }
    public UserRole Role { get; init; } = UserRole.Examiner;
}
