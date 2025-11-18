using Exam.Domain.Entities;
using Exam.Services.Models.Responses.ExamSubjects;

namespace Exam.Services.Mappers;

public static class ExamSubjectMapper
{
    public static ExamSubjectResponse ToExamSubjectResponse(this ExamSubject examSubject)
    {
        return new ExamSubjectResponse
        {
            Id = examSubject.Id,
            ExamId = examSubject.ExamId,
            SubjectId = examSubject.SubjectId,
            ExamCode = examSubject.Exam?.Code ?? string.Empty,
            SubjectCode = examSubject.Subject?.Code ?? string.Empty,
            ScoreStructure = examSubject.ScoreStructure,
            ViolationStructure = examSubject.ViolationStructure,
            CreatedAt = examSubject.CreatedAt,
            UpdatedAt = examSubject.UpdatedAt,
            IsActive = examSubject.IsActive
        };
    }
}
