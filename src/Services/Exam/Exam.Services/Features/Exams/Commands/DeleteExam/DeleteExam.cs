using Exam.Repositories.Interfaces.Repositories;
using Exam.Services.Exceptions;
using Exam.Services.Models.Responses;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Exam.Services.Features.Exams.Commands.DeleteExam;

public record DeleteExamCommand(Guid Id) : IRequest<BaseServiceResponse>;

public class DeleteExamCommandValidator : AbstractValidator<DeleteExamCommand>
{
    public DeleteExamCommandValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("Vui lòng nhập Id kỳ thi!");
    }
}

public class DeleteExamCommandHandler : IRequestHandler<DeleteExamCommand, BaseServiceResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteExamCommandHandler> _logger;

    public DeleteExamCommandHandler(IUnitOfWork unitOfWork, ILogger<DeleteExamCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BaseServiceResponse> Handle(DeleteExamCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.GetRepository<Domain.Entities.Exam>();
        var exam = await repository
            .Query()
            .Where(e => e.IsActive)
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
        if (exam == null)
        {
            _logger.LogError("Failed to retrieve exam with ID: {Id}", request.Id);
            throw new NotFoundException("Không tìm thấy kỳ thi!");
        }

        exam.IsActive = false;
        await repository.UpdateAsync(exam, cancellationToken);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Exam deleted successfully with ID: {Id}", exam.Id);
        return new BaseServiceResponse() { Success = true, Message = "Xóa kỳ thi thành công!" };
    }
}
