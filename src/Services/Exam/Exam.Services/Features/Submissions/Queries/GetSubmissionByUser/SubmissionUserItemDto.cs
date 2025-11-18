using Exam.Domain.Enums;

namespace Exam.Services.Features.Submissions.Queries.GetSubmissionByUser;

public class SubmissionUserItemDto
{
    public Guid Id { get; set; }
    public Guid ExamSubjectId { get; set; }
    public Guid ExamId { get; set; }
    public string? ExamCode { get; set; }
    public Guid SubjectId { get; set; }
    public string? SubjectIdCode { get; set; }

    public DateTimeOffset AssignAt { get; set; }
    
    public SubmissionStatus Status { get; set; }
    public GradeStatus GradeStatus { get; set; }
    public string? FileUrl { get; set; }
    
    public Guid? AssessmentId { get; set; }
    public string? SubmissionName { get; set; }
    public AssessmentStatus? AssessmentStatus { get; set; }
    public decimal? MyScore { get; set; }
}
