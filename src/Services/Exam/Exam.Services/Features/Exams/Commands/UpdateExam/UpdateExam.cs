using Exam.Domain.Entities;
using Exam.Repositories.Interfaces.Repositories;
using Exam.Services.Exceptions;
using Exam.Services.Mappers;
using Exam.Services.Models.Responses;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Exam.Services.Features.Exams.Commands.UpdateExam;

public record UpdateExamCommand : IRequest<BaseServiceResponse>
{
    public Guid Id { get; init; }
    public Guid SemesterId { get; init; }
    public string Code { get; init; } = null!;
    public DateTimeOffset StartDate { get; init; }
    public DateTimeOffset EndDate { get; init; }
}

public class UpdateExamCommandValidator : AbstractValidator<UpdateExamCommand>
{
    public UpdateExamCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Vui lòng nhập ID kỳ thi!");

        RuleFor(x => x.SemesterId)
            .NotEmpty().WithMessage("Vui lòng nhập mã học kỳ!");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Vui lòng nhập mã kỳ thi!");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Vui lòng nhập ngày bắt đầu kỳ thi!")
            .LessThan(x => x.EndDate).WithMessage("Ngày bắt đầu kỳ thi phải trước ngày kết thúc kỳ thi!");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("Vui lòng nhập ngày kết thúc kỳ thi!")
            .GreaterThan(x => x.StartDate).WithMessage("Ngày kết thúc kỳ thi phải sau ngày bắt đầu kỳ thi!");
    }
}

public class UpdateExamCommandHandler : IRequestHandler<UpdateExamCommand, BaseServiceResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateExamCommandHandler> _logger;

    public UpdateExamCommandHandler(IUnitOfWork unitOfWork, ILogger<UpdateExamCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BaseServiceResponse> Handle(UpdateExamCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.GetRepository<Domain.Entities.Exam>();
        var exam = await repository.Query()
            .Where(e => e.IsActive)
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
        if (exam == null)
        {
            _logger.LogError("Failed to retrieve exam with ID: {Id}", request.Id);
            throw new NotFoundException("Khong tìm thấy kỳ thi!");
        }

        var semester = await _unitOfWork.GetRepository<Semester>()
            .Query()
            .Where(e => e.IsActive)
            .FirstOrDefaultAsync(s => s.Id == request.SemesterId, cancellationToken);
        if (semester == null)
        {
            _logger.LogError("Failed to retrieve semester with ID: {Id}", request.SemesterId);
            throw new NotFoundException("Không tìm thấy học kỳ!");
        }

        exam.UpdateExam(request);
        await repository.UpdateAsync(exam, cancellationToken);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Exam updated successfully with ID: {Id}", exam.Id);
        return new BaseServiceResponse()
        {
            Success = true,
            Message = "Chỉnh sửa đề kỳ thi thành công!",
        };
    }
}
