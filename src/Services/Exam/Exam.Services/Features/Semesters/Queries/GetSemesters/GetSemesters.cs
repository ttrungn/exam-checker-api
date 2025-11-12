using Exam.Services.Interfaces.Services;
using Exam.Services.Models.Responses;
using Exam.Services.Models.Responses.Semesters;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Exam.Services.Features.Semesters.Queries.GetSemesters;

public record GetSemestersQuery : IRequest<BaseServiceResponse>
{
    public string? Name { get; init; }
    public bool? IsActive { get; init; }
    public int PageIndex { get; init; }
    public int PageSize { get; init; }
}

public class GetSemestersQueryValidator : AbstractValidator<GetSemestersQuery>
{
    public GetSemestersQueryValidator()
    {
        RuleFor(x => x.PageIndex)
            .GreaterThanOrEqualTo(0).WithMessage("Vui lòng nhập số trang lớn hơn hoặc 0!");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Vui lòng nhập kỳ lớn hơn 0!");
    }
}

public class GetSemestersQueryHandler : IRequestHandler<GetSemestersQuery, BaseServiceResponse>
{
    private readonly ISemesterService _semesterService;
    private readonly ILogger<GetSemestersQueryHandler> _logger;

    public GetSemestersQueryHandler(ISemesterService semesterService, ILogger<GetSemestersQueryHandler> logger)
    {
        _semesterService = semesterService;
        _logger = logger;
    }

    public async Task<BaseServiceResponse> Handle(GetSemestersQuery request, CancellationToken cancellationToken)
    {
        var response = await _semesterService.GetSemestersAsync(request, cancellationToken);
        if (response.Success  && response is PaginationServiceResponse<SemesterResponse> paginationResponse)
        {
            _logger.LogInformation("Semesters retrieved successfully!");
            return paginationResponse;
        }

        _logger.LogError("Failed to retrieve semesters: {ErrorMessage}", response.Message);
        return response;
    }
}
