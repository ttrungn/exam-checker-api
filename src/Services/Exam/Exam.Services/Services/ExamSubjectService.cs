using System.Text.Json;
using Exam.Domain.Entities;
using Exam.Repositories.Interfaces.Repositories;
using Exam.Services.Exceptions;
using Exam.Services.Interfaces.Services;
using Exam.Services.Models.Responses;
using Exam.Services.Models.Validations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Exam.Services.Services;

public class ExamSubjectService : IExamSubjectService
{
    private readonly ILogger<ExamSubjectService> _logger;
    
    private readonly IUnitOfWork _unitOfWork;

    public ExamSubjectService(
        ILogger<ExamSubjectService> logger,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }   
    
    
    public async Task<BaseServiceResponse> UpdateViolationStructureAsync(Guid examSubjectId, ValidationRules rules)
    {
        _logger.LogInformation("Updating violation structure");
        
        
        var examSubjectRepo = _unitOfWork.GetRepository<ExamSubject>();

        var examSubject = await examSubjectRepo.Query().FirstOrDefaultAsync(ex => ex.Id  == examSubjectId);

        if (examSubject is null)
        {
            throw new NotFoundException($"Exam subject not found for Exam Subject Id:{examSubjectId}");
        }
        
        examSubject.ViolationStructure = JsonSerializer.Serialize(rules);
        
        await examSubjectRepo.UpdateAsync(examSubject);
        await _unitOfWork.SaveChangesAsync();
        return new BaseServiceResponse()
        {
            Message = "Update Violation Structure successfully",
            Success = true,
        };
    }
}
