
using Exam.Services.Features.Submission.Commands.CreateSubmissionsFromZipCommand;
using Exam.Services.Models.Responses;

namespace Exam.Services.Interfaces.Services;

public interface ISubmissionService
{
    Task<DataServiceResponse<List<Guid>>> CreateSubmissionsFromZipAsync(
        CreateSubmissionsFromZipCommand command,
        CancellationToken cancellationToken = default);
    
}
