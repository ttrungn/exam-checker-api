using Exam.Domain.Entities;
using Exam.Services.Models.Responses.ExamSubjects;
using Microsoft.Graph.Models;

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
            StartDate = examSubject.Exam!.StartDate,
            EndDate = examSubject.Exam!.EndDate,
            IsActive = examSubject.IsActive
        };
    }
}
