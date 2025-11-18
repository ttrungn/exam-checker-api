using Exam.Domain.Entities;
using Exam.Domain.Enums;
using Exam.Repositories.Interfaces.Repositories;
using Exam.Services.Exceptions;
using Exam.Services.Models.Responses;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Exam.Services.Features.Submissions.Commands.UpdateToModeratorViolated;

public record UpdateToModeratorViolatedCommand(Guid Id) : IRequest<BaseServiceResponse>;

public class UpdateToModeratorViolatedCommandValidator : AbstractValidator<UpdateToModeratorViolatedCommand>
{
    public UpdateToModeratorViolatedCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Xin vui lòng nhập ID!");
    }
}

public class UpdateToModeratorViolatedCommandHandler : IRequestHandler<UpdateToModeratorViolatedCommand, BaseServiceResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateToModeratorViolatedCommandHandler> _logger;

    public UpdateToModeratorViolatedCommandHandler(IUnitOfWork unitOfWork, ILogger<UpdateToModeratorViolatedCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BaseServiceResponse> Handle(UpdateToModeratorViolatedCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.GetRepository<Submission>();
        var submission = await repository.Query().FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (submission == null)
        {
            _logger.LogError("Failed to retrieve submission with ID: {Id}", request.Id);
            throw new NotFoundException("Không tìm thấy bài nộp!");
        }

        submission.Status = SubmissionStatus.ModeratorViolated;
        await repository.UpdateAsync(submission, cancellationToken);
        await _unitOfWork.SaveChangesAsync();

        return new BaseServiceResponse()
        {
            Success = true,
            Message = "Cập nhật trạng thái bài nộp thành công!",
        };
    }
}
