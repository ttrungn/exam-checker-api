using System.Text.Json;
using Exam.Domain.Entities;
using Exam.Repositories.Interfaces.Repositories;
using Exam.Services.Exceptions;
using Exam.Services.Features.ExamSubjects.Queries.GetExamSubjects;
using Exam.Services.Interfaces.Services;
using Exam.Services.Mappers;
using Exam.Services.Models.Responses;
using Exam.Services.Models.Responses.ExamSubjects;
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
        _logger.LogInformation("Updating violation structure for ExamSubject {ExamSubjectId}", examSubjectId);
        
        var examSubjectRepo = _unitOfWork.GetRepository<ExamSubject>();
        var examSubject = await examSubjectRepo.Query().FirstOrDefaultAsync(ex => ex.Id == examSubjectId);

        if (examSubject is null)
        {
            throw new NotFoundException($"Exam subject not found for Exam Subject Id:{examSubjectId}");
        }
        
        examSubject.ViolationStructure = JsonSerializer.Serialize(rules);
        
        await examSubjectRepo.UpdateAsync(examSubject);
        await _unitOfWork.SaveChangesAsync();
        
        return new BaseServiceResponse
        {
            Message = "Update Violation Structure successfully",
            Success = true,
        };
    }

    public async Task<BaseServiceResponse> GetExamSubjectsAsync(GetExamSubjectsQuery query)
    {
        _logger.LogInformation("Getting ExamSubjects with filters");
        
        var repository = _unitOfWork.GetRepository<ExamSubject>();
        var dbQuery = repository.Query()
            .Include(es => es.Subject)
            .Include(es => es.Exam)
            .AsNoTracking();

        if (!string.IsNullOrEmpty(query.ExamCode))
        {
            dbQuery = dbQuery.Where(es => es.Exam.Code.Equals(query.ExamCode,StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(query.SubjectCode))
        {
            dbQuery = dbQuery.Where(es => es.Subject.Code.Equals(query.SubjectCode, StringComparison.OrdinalIgnoreCase));
        }

        if (query.IsActive.HasValue)
        {
            dbQuery = dbQuery.Where(es => es.IsActive == query.IsActive.Value);
        }

        var totalCount = await dbQuery.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);
        
        var examSubjects = await dbQuery
            .OrderByDescending(es => es.CreatedAt)
            .Skip((query.PageIndex - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();
        
        var responses = examSubjects.Select(es => es.ToExamSubjectResponse()).ToList();
        
        return new PaginationServiceResponse<ExamSubjectResponse>
        {
            Success = true,
            Message = "Lấy danh sách ExamSubject thành công!",
            PageIndex = query.PageIndex,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            TotalCurrentCount = examSubjects.Count,
            TotalPages = totalPages,
            Data = responses
        };
    }

    public async Task<BaseServiceResponse> GetExamSubjectByIdAsync(Guid id)
    {
        _logger.LogInformation("Getting ExamSubject {Id}", id);
        
        var repository = _unitOfWork.GetRepository<ExamSubject>();
        var examSubject = await repository
            .Query()
            .Include(es => es.Subject)
            .Include(es => es.Exam)
            .AsNoTracking()
            .FirstOrDefaultAsync(es => es.Id == id);

        if (examSubject is null)
        {
            throw new NotFoundException($"ExamSubject với Id {id} không tồn tại!");
        }

        var response = examSubject.ToExamSubjectResponse();
        
        return new DataServiceResponse<ExamSubjectResponse>
        {
            Success = true,
            Message = "Lấy ExamSubject thành công!",
            Data = response
        };
    }
}
