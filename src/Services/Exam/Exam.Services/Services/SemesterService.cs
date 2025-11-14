using Exam.Domain.Entities;
using Exam.Repositories.Interfaces.Repositories;
using Exam.Services.Exceptions;
using Exam.Services.Features.Semesters.Commands.CreateSemester;
using Exam.Services.Features.Semesters.Commands.UpdateSemester;
using Exam.Services.Features.Semesters.Queries.GetSemesters;
using Exam.Services.Interfaces.Services;
using Exam.Services.Mappers;
using Exam.Services.Models.Responses;
using Exam.Services.Models.Responses.Semesters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Exam.Services.Services;

public class SemesterService : ISemesterService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SemesterService> _logger;

    public SemesterService(IUnitOfWork unitOfWork, ILogger<SemesterService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BaseServiceResponse> CreateAsync(
        CreateSemesterCommand request,
        CancellationToken cancellationToken = default)
    {
        var semester = request.ToSemester();
        await _unitOfWork.GetRepository<Semester>().InsertAsync(semester, cancellationToken);
        await _unitOfWork.SaveChangesAsync();
        return new DataServiceResponse<Guid>()
        {
            Success = true, Message = "Tạo học kỳ thành công!", Data = semester.Id
        };
    }

    public async Task<BaseServiceResponse> UpdateAsync(
        UpdateSemesterCommand request,
        CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.GetRepository<Semester>();
        var semester = await repository
            .Query()
            .Where(s => s.IsActive)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (semester == null)
        {
            _logger.LogError("Failed to retrieve semester with ID: {Id}", request.Id);
            throw new NotFoundException("Không tìm thấy học kỳ!");
        }

        semester.UpdateSemester(request);
        await repository.UpdateAsync(semester, cancellationToken);
        await _unitOfWork.SaveChangesAsync();
        return new DataServiceResponse<Guid>()
        {
            Success = true, Message = "Chỉnh sửa học kỳ thành công!", Data = semester.Id
        };
    }

    public async Task<BaseServiceResponse> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.GetRepository<Semester>();
        var semester = await repository
            .Query()
            .Where(s => s.IsActive)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (semester == null)
        {
            _logger.LogError("Failed to retrieve semester with ID: {Id}", id);
            throw new NotFoundException("Không tìm thấy học kỳ!");
        }

        semester.IsActive = false;
        await repository.UpdateAsync(semester, cancellationToken);
        await _unitOfWork.SaveChangesAsync();
        return new DataServiceResponse<Guid>()
        {
            Success = true, Message = "Xoá học kỳ thành công!", Data = semester.Id
        };
    }

    public async Task<BaseServiceResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.GetRepository<Semester>();
        var semester = await repository.Query().FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (semester == null)
        {
            _logger.LogError("Failed to retrieve semester with ID: {Id}", id);
            throw new NotFoundException("Không tìm thấy học kỳ!");
        }

        return new DataServiceResponse<SemesterResponse>()
        {
            Success = true, Message = "Lấy học kỳ thành công!", Data = semester.ToSemesterResponse()
        };
    }

    public async Task<BaseServiceResponse> GetSemestersAsync(GetSemestersQuery request, CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.GetRepository<Semester>();
        var query = repository.Query().AsNoTracking();

        if (!string.IsNullOrEmpty(request.Name))
        {
            query = query.Where(s => s.Name.Contains(request.Name));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(s => s.IsActive == request.IsActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);
        var semesters = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
        var totalCurrentCount = semesters.Count;

        var responses = semesters.Select(s => s.ToSemesterResponse()).ToList();
        return new PaginationServiceResponse<SemesterResponse>()
        {
            Success = true,
            Message = "Lấy danh sách học kỳ thành công!",
            PageIndex =  request.PageIndex,
            PageSize = request.PageSize,
            TotalCount =  totalCount,
            TotalCurrentCount = totalCurrentCount,
            TotalPages = totalPages,
            Data = responses,
        };
    }
}
