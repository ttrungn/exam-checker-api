using Exam.Services.Interfaces.Services;
using Exam.Services.Models.Responses;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Exam.Services.Features.Semesters.Commands.CreateSemester;

public record CreateSemesterCommand : IRequest<BaseServiceResponse>
{
    public string Name { get; init; } = null!;
}

public class CreateSemesterCommandValidator : AbstractValidator<CreateSemesterCommand>
{
    public CreateSemesterCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Vui lòng nhập tên!");
    }
}

public class CreateSemesterCommandHandler : IRequestHandler<CreateSemesterCommand, BaseServiceResponse>
{
    private readonly ISemesterService _semesterService;
    private readonly ILogger<CreateSemesterCommandHandler> _logger;

    public CreateSemesterCommandHandler(ISemesterService semesterService, ILogger<CreateSemesterCommandHandler> logger)
    {
        _semesterService = semesterService;
        _logger = logger;
    }

    public async Task<BaseServiceResponse> Handle(CreateSemesterCommand request, CancellationToken cancellationToken)
    {
        var response = await _semesterService.CreateAsync(request, cancellationToken);
        if (response.Success && response is DataServiceResponse<Guid> dataResponse)
        {
            _logger.LogInformation("Semester created successfully with ID: {SemesterId}", dataResponse.Data);
            return dataResponse;
        }

        _logger.LogError("Failed to create semester: {ErrorMessage}", response.Message);
        return new BaseServiceResponse()
        {
            Success = false,
            Message = "Tạo học kỳ thất bại!"
        };
    }
}
