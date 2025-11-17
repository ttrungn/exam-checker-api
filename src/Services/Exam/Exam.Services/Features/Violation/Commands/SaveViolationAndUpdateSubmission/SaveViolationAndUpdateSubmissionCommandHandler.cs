using Exam.Services.Interfaces.Services;
using Exam.Services.Models.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Exam.Services.Features.Violation.Commands.SaveViolationAndUpdateSubmission;

public class SaveViolationAndUpdateSubmissionCommandHandler : IRequestHandler<SaveViolationAndUpdateSubmissionCommand, BaseServiceResponse>
{
    private readonly IViolationService _violationService;
    private readonly ILogger<SaveViolationAndUpdateSubmissionCommandHandler> _logger;

    public SaveViolationAndUpdateSubmissionCommandHandler(
        IViolationService violationService,
        ILogger<SaveViolationAndUpdateSubmissionCommandHandler> logger)
    {
        _violationService = violationService;
        _logger = logger;
    }

    public async Task<BaseServiceResponse> Handle(SaveViolationAndUpdateSubmissionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var violations = request.Violations.Select(v => new Domain.Entities.Violation
            {
                SubmissionId = request.SubmissionId,
                ViolationType = v.ViolationType,
                Description = v.Description
            }).ToList();

            await _violationService.SaveViolationsAndUpdateSubmissionAsync(
                request.SubmissionId,
                violations,
                cancellationToken);

            _logger.LogInformation("Violations saved successfully for submission {SubmissionId}", request.SubmissionId);

            return new BaseServiceResponse
            {
                Success = true,
                Message = "Violations saved and submission status updated successfully."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving violations for submission {SubmissionId}", request.SubmissionId);
            return new BaseServiceResponse
            {
                Success = false,
                Message = $"Failed to save violations: {ex.Message}"
            };
        }
    }
}
