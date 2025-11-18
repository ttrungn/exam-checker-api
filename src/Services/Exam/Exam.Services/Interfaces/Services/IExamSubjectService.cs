using Exam.Domain.Entities;
using Exam.Services.Models.Responses;
using Exam.Services.Models.Validations;

namespace Exam.Services.Interfaces.Services;

public interface IExamSubjectService
{
    Task<BaseServiceResponse> UpdateViolationStructureAsync(Guid examSubjectId, ValidationRules rules);

}
