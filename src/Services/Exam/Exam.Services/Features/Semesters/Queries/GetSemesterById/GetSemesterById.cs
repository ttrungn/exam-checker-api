using Exam.Services.Interfaces.Services;
using Exam.Services.Models.Responses;
using Exam.Services.Models.Responses.Semesters;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Exam.Services.Features.Semesters.Queries.GetSemesterById;

public record GetSemesterByIdQuery(Guid Id) : IRequest<BaseServiceResponse>;

public class GetSemesterByIdQueryValidator : AbstractValidator<GetSemesterByIdQuery>
{
    public GetSemesterByIdQueryValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("Vui lòng nhập Id!");
    }
}

public class GetSemesterByIdQueryHandler : IRequestHandler<GetSemesterByIdQuery, BaseServiceResponse>
{
    private readonly ISemesterService _semesterService;
    private readonly ILogger<GetSemesterByIdQueryHandler> _logger;

    public GetSemesterByIdQueryHandler(ISemesterService semesterService, ILogger<GetSemesterByIdQueryHandler> logger)
    {
        _semesterService = semesterService;
        _logger = logger;
    }

    public async Task<BaseServiceResponse> Handle(GetSemesterByIdQuery request, CancellationToken cancellationToken)
    {
        var response = await _semesterService.GetByIdAsync(request.Id, cancellationToken);
        if (response.Success  && response is DataServiceResponse<SemesterResponse> dataResponse)
        {
            _logger.LogInformation("Semester retrieved successfully with ID: {SemesterId}", dataResponse.Data.Id);
            return dataResponse;
        }

        _logger.LogError("Failed to retrieve semester: {ErrorMessage}", response.Message);
        return response;
    }
}
