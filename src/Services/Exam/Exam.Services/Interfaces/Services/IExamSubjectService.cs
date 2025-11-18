using Exam.Services.Features.ExamSubjects.Queries.GetExamSubjects;
using Exam.Services.Models.Responses;
using Exam.Services.Models.Validations;

namespace Exam.Services.Interfaces.Services;

public interface IExamSubjectService
{
    Task<BaseServiceResponse> UpdateViolationStructureAsync(Guid examSubjectId, ValidationRules rules);
    Task<BaseServiceResponse> GetExamSubjectsAsync(GetExamSubjectsQuery query);
    Task<BaseServiceResponse> GetExamSubjectByIdAsync(Guid id);
}
