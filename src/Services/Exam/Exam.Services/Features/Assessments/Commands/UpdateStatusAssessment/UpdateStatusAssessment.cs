using Exam.Domain.Entities;
using Exam.Domain.Enums;
using Exam.Repositories.Interfaces.Repositories;
using Exam.Services.Exceptions;
using Exam.Services.Models.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Exam.Services.Features.Assessments.Commands.UpdateStatusAssessment;

public record UpdateStatusAssessmentCommand : IRequest<BaseServiceResponse>
{
    public Guid Id { get; init; }
    public AssessmentStatus Status { get; init; }
}

public class UpdateStatusAssessmentCommandHandler 
    : IRequestHandler<UpdateStatusAssessmentCommand, BaseServiceResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateStatusAssessmentCommandHandler> _logger;

    public UpdateStatusAssessmentCommandHandler(IUnitOfWork unitOfWork, ILogger<UpdateStatusAssessmentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger     = logger;
    }

    public async Task<BaseServiceResponse> Handle(
        UpdateStatusAssessmentCommand request,
        CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.GetRepository<Assessment>();

        var assessment = await repo.Query()
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (assessment == null)
        {
            _logger.LogWarning("Assessment with ID {AssessmentId} not found.", request.Id);
            throw new NotFoundException("Assessment not found.");
        }

        assessment.Status = request.Status;

        await repo.UpdateAsync(assessment, cancellationToken);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Assessment with ID {AssessmentId} successfully updated.", request.Id);
        return new BaseServiceResponse
        {
            Success = true,
            Message = "Assessment status updated successfully."
        };
    }
}
