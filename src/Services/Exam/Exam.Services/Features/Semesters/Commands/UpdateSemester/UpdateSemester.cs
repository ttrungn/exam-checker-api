using Exam.Services.Interfaces.Services;
using Exam.Services.Models.Responses;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Exam.Services.Features.Semesters.Commands.UpdateSemester;

public record UpdateSemesterCommand : IRequest<BaseServiceResponse>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
}

public class UpdateSemesterCommandValidator : AbstractValidator<UpdateSemesterCommand>
{
    public UpdateSemesterCommandValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("Vui lòng nhập Id!");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Vui lòng nhập tên!");
    }
}

public class UpdateSemesterCommandHandler : IRequestHandler<UpdateSemesterCommand, BaseServiceResponse>
{
    private readonly ISemesterService _semesterService;
    private readonly ILogger<UpdateSemesterCommandHandler> _logger;

    public UpdateSemesterCommandHandler(ISemesterService semesterService, ILogger<UpdateSemesterCommandHandler> logger)
    {
        _semesterService = semesterService;
        _logger = logger;
    }

    public async Task<BaseServiceResponse> Handle(UpdateSemesterCommand request, CancellationToken cancellationToken)
    {
        var response = await _semesterService.UpdateAsync(request, cancellationToken);
        if (response.Success  && response is DataServiceResponse<Guid> dataResponse)
        {
            _logger.LogInformation("Semester updated successfully with ID: {SemesterId}", dataResponse.Data);
            return dataResponse;
        }

        _logger.LogError("Failed to update semester: {ErrorMessage}", response.Message);
        return response;
    }
}
