using Exam.Domain.Entities;
using Exam.Repositories.Interfaces.Repositories;
using Exam.Services.Exceptions;
using Exam.Services.Models.Responses;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Exam.Services.Features.Subjects.Commands.DeleteSubject;

public record DeleteSubjectCommand(Guid Id) : IRequest<BaseServiceResponse>;

public class DeleteSubjectCommandValidator : AbstractValidator<DeleteSubjectCommand>
{
    public DeleteSubjectCommandValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("Vui lòng nhập Id!");
    }
}

public class DeleteSubjectCommandHandler : IRequestHandler<DeleteSubjectCommand, BaseServiceResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteSubjectCommandHandler> _logger;

    public DeleteSubjectCommandHandler(IUnitOfWork unitOfWork, ILogger<DeleteSubjectCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BaseServiceResponse> Handle(DeleteSubjectCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.GetRepository<Subject>();
        var subject = await repository.Query().FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (subject == null)
        {
            _logger.LogError("Failed to retrieve subject with ID: {Id}", request.Id);
            throw new NotFoundException("Không tìm thấy môn học!");
        }

        subject.IsActive = false;
        await repository.UpdateAsync(subject, cancellationToken);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Subject deleted successfully with ID: {Id}", subject.Id);
        return new BaseServiceResponse()
        {
            Success = true,
            Message = "Xóa môn học thành công!"
        };
    }
}
