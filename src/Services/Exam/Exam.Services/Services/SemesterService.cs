using Exam.Domain.Entities;
using Exam.Repositories.Interfaces.Repositories;
using Exam.Services.Features.Semesters.Commands.CreateSemester;
using Exam.Services.Features.Semesters.Commands.UpdateSemester;
using Exam.Services.Features.Semesters.Queries.GetSemesters;
using Exam.Services.Interfaces.Services;
using Exam.Services.Mappers;
using Exam.Services.Models.Responses;
using Exam.Services.Models.Responses.Semesters;
using Microsoft.EntityFrameworkCore;

namespace Exam.Services.Services;

public class SemesterService : ISemesterService
{
    private readonly IUnitOfWork _unitOfWork;

    public SemesterService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
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
        UpdateSemesterCommand reqest,
        CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.GetRepository<Semester>();
        var semester = await repository.Query().FirstOrDefaultAsync(s => s.Id == reqest.Id, cancellationToken);
        if (semester == null)
        {
            return new BaseServiceResponse() { Success = false, Message = "Không tìm thấy học kỳ!" };
        }

        semester.UpdateSemester(reqest);
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
        var semester = await repository.Query().FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (semester == null)
        {
            return new BaseServiceResponse() { Success = false, Message = "Không tìm thấy học kỳ!" };
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
            return new BaseServiceResponse() { Success = false, Message = "Không tìm thấy học kỳ!" };
        }

        return new DataServiceResponse<SemesterResponse>()
        {
            Success = true, Message = "Lấy học kỳ thành công!", Data = semester.ToSemesterResponse()
        };
    }

    public async Task<BaseServiceResponse> GetSemestersAsync(GetSemestersQuery reqest, CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.GetRepository<Semester>();
        var query = repository.Query().AsNoTracking();

        if (!string.IsNullOrEmpty(reqest.Name))
        {
            query = query.Where(s => s.Name.Contains(reqest.Name));
        }

        if (reqest.IsActive.HasValue)
        {
            query = query.Where(s => s.IsActive == reqest.IsActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling((double)totalCount / reqest.PageSize);
        var semesters = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((reqest.PageIndex - 1)* reqest.PageSize)
            .Take(reqest.PageSize)
            .ToListAsync(cancellationToken);

        var responses = semesters.Select(s => s.ToSemesterResponse()).ToList();
        return new PaginationServiceResponse<SemesterResponse>()
        {
            Success = true,
            Message = "Lấy danh sách học kỳ thành công!",
            PageIndex =  reqest.PageIndex,
            PageSize = reqest.PageSize,
            TotalCount =  totalCount,
            TotalPages = totalPages,
            Data = responses,
        };
    }
}
