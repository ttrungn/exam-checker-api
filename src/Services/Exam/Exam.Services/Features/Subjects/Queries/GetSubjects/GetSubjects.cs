using Exam.Domain.Entities;
using Exam.Repositories.Interfaces.Repositories;
using Exam.Services.Mappers;
using Exam.Services.Models.Responses;
using Exam.Services.Models.Responses.Subjects;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Exam.Services.Features.Subjects.Queries.GetSubjects;

public record GetSubjectsQuery : IRequest<BaseServiceResponse>
{
    public string? Name { get; init; }
    public string? Code { get; init; } = null!;
    public bool? IsActive { get; init; }
    public int PageIndex { get; init; }
    public int PageSize { get; init; }
}

public class GetSubjectsQueryValidator : AbstractValidator<GetSubjectsQuery>
{
    public GetSubjectsQueryValidator()
    {
        RuleFor(x => x.PageIndex)
            .GreaterThanOrEqualTo(0).WithMessage("Vui lòng nhập số trang lớn hơn hoặc 0!");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Vui lòng nhập kỳ lớn hơn 0!");
    }
}

class GetSubjectsQueryHandler : IRequestHandler<GetSubjectsQuery, BaseServiceResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetSubjectsQueryHandler> _logger;

    public GetSubjectsQueryHandler(IUnitOfWork unitOfWork, ILogger<GetSubjectsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BaseServiceResponse> Handle(GetSubjectsQuery request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.GetRepository<Subject>();

        var query = repository.Query().AsNoTracking();

        if (!string.IsNullOrEmpty(request.Name))
        {
            query = query.Where(s => s.Name.Contains(request.Name));
        }

        if (!string.IsNullOrEmpty(request.Code))
        {
            query = query.Where(s => s.Code.Contains(request.Code));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(s => s.IsActive == request.IsActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
        var subjects = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
        var totalCurrentCount = subjects.Count;

        var responses = subjects.Select(s => s.ToSubjectResponse()).ToList();
        _logger.LogInformation("Subjects retrieved successfully!");
        return new PaginationServiceResponse<SubjectResponse>()
        {
            Success = true,
            Message = "Lấy danh sách học kỳ thành công!",
            PageIndex = request.PageIndex,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalCurrentCount = totalCurrentCount,
            TotalPages = totalPages,
            Data = responses,
        };
    }
}
