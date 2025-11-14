using Exam.Repositories.Interfaces.Repositories;
using Exam.Services.Mappers;
using Exam.Services.Models.Responses;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Exam.Services.Features.Exams.Commands.CreateExam;

public record CreateExamCommand : IRequest<BaseServiceResponse>
{
    public Guid SemesterId { get; init; }
    public string Code { get; init; } = null!;
    public DateTimeOffset StartDate { get; init; }
    public DateTimeOffset EndDate { get; init; }
}

public class CreateExamCommandValidator : AbstractValidator<CreateExamCommand>
{
    public CreateExamCommandValidator()
    {
        RuleFor(x => x.SemesterId)
            .NotEmpty().WithMessage("Vui lòng nhập mã học kỳ!");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Vui lòng nhập kỳ thi!");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Vui lòng nhập ngày bắt đầu kỳ thi!")
            .LessThan(x => x.EndDate).WithMessage("Ngày bắt đầu kỳ thi phải trước ngày kết thúc kỳ thi!");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("Vui lòng nhập ngày kết thúc kỳ thi!")
            .GreaterThan(x => x.StartDate).WithMessage("Ngày kết thúc kỳ thi phải sau ngày bắt đầu kỳ thi!");
    }
}

public class CreateExamCommandHandler : IRequestHandler<CreateExamCommand, BaseServiceResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateExamCommandHandler> _logger;

    public CreateExamCommandHandler(IUnitOfWork unitOfWork, ILogger<CreateExamCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BaseServiceResponse> Handle(CreateExamCommand request, CancellationToken cancellationToken)
    {
        var semester = await _unitOfWork.GetRepository<Domain.Entities.Semester>()
            .Query()
            .FirstOrDefaultAsync(s => s.Id == request.SemesterId, cancellationToken);
        if (semester == null)
        {
            _logger.LogError("Failed to retrieve semester with ID: {Id}", request.SemesterId);
            return new BaseServiceResponse() { Success = false, Message = "Không tìm thấy học kỳ!" };
        }

        var exam = request.ToExam();
        await _unitOfWork.GetRepository<Domain.Entities.Exam>().InsertAsync(exam, cancellationToken);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Exam created successfully with ID: {Id}", exam.Id);
        return new DataServiceResponse<Guid>()
        {
            Success = true,
            Message = "Tạo kỳ thi thành công!",
            Data = exam.Id
        };
    }
}
