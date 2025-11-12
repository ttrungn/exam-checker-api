using Exam.Services.Features.Semesters.Commands.CreateSemester;
using Exam.Services.Features.Semesters.Commands.UpdateSemester;
using Exam.Services.Features.Semesters.Queries.GetSemesters;
using Exam.Services.Models.Responses;

namespace Exam.Services.Interfaces.Services;

public interface ISemesterService
{
    Task<BaseServiceResponse> CreateAsync(
        CreateSemesterCommand reqest,
        CancellationToken cancellationToken = default);

    Task<BaseServiceResponse> UpdateAsync(
        UpdateSemesterCommand reqest,
        CancellationToken cancellationToken = default);

    Task<BaseServiceResponse> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<BaseServiceResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<BaseServiceResponse> GetSemestersAsync(
        GetSemestersQuery reqest,
        CancellationToken cancellationToken = default);
}
