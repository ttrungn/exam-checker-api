using Exam.Repositories.Interfaces.Repositories;
using Exam.Services.Mappers;
using Exam.Services.Models.Responses;
using Exam.Services.Models.Responses.Exams;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Exam.Services.Features.Exams.Queries.GetExams;

public record GetExamsQuery : IRequest<BaseServiceResponse>
{
    public string? Code { get; init; }
    public bool? IsActive { get; init; }
    public int PageIndex { get; init; }
    public int PageSize { get; init; }
}

public class GetExamsQueryValidator : AbstractValidator<GetExamsQuery>
{
    public GetExamsQueryValidator()
    {
        RuleFor(x => x.PageIndex)
            .GreaterThanOrEqualTo(0).WithMessage("Vui lòng nhập số trang lớn hơn hoặc 0!");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Vui lòng nhập số lượng lớn hơn 0!");
    }
}

public class GetExamsQueryHandler : IRequestHandler<GetExamsQuery, BaseServiceResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetExamsQueryHandler> _logger;

    public GetExamsQueryHandler(IUnitOfWork unitOfWork, ILogger<GetExamsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BaseServiceResponse> Handle(GetExamsQuery request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.GetRepository<Domain.Entities.Exam>();

        var query = repository.Query().AsNoTracking();

        if (!string.IsNullOrEmpty(request.Code))
        {
            query = query.Where(e => e.Code.Contains(request.Code));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(e => e.IsActive == request.IsActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
        var exams = await query
            .OrderByDescending(e => e.EndDate)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
        var totalCurrentCount = exams.Count;

        var responses = exams.Select(e => e.ToExamResponse()).ToList();
        _logger.LogInformation("Exams retrieved successfully!");
        return new PaginationServiceResponse<ExamResponse>()
        {
            Success = true,
            Message = "Lấy danh sách kỳ thi thành công!",
            PageIndex = request.PageIndex,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalCurrentCount = totalCurrentCount,
            TotalPages = totalPages,
            Data = responses
        };
    }
}
