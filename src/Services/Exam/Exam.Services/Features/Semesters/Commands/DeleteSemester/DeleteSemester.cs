using Exam.Services.Interfaces.Services;
using Exam.Services.Models.Responses;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Exam.Services.Features.Semesters.Commands.DeleteSemester;

public record DeleteSemesterCommand(Guid Id) : IRequest<BaseServiceResponse>;


public class DeleteSemesterCommandValidator : AbstractValidator<DeleteSemesterCommand>
{
    public DeleteSemesterCommandValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("Vui lòng nhập Id!");
    }
}

public class DeleteSemesterCommandHandler : IRequestHandler<DeleteSemesterCommand, BaseServiceResponse>
{
    private readonly ISemesterService _semesterService;
    private readonly ILogger<DeleteSemesterCommandHandler> _logger;

    public DeleteSemesterCommandHandler(ISemesterService semesterService, ILogger<DeleteSemesterCommandHandler> logger)
    {
        _semesterService = semesterService;
        _logger = logger;
    }

    public async Task<BaseServiceResponse> Handle(DeleteSemesterCommand request, CancellationToken cancellationToken)
    {
        var response = await _semesterService.DeleteAsync(request.Id, cancellationToken);
        if (response.Success  && response is DataServiceResponse<Guid> dataResponse)
        {
            _logger.LogInformation("Semester deleted successfully with ID: {SemesterId}", dataResponse.Data);
            return dataResponse;
        }

        _logger.LogError("Failed to delete semester: {ErrorMessage}", response.Message);
        return response;
    }
}
