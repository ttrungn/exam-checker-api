using Exam.Domain.Entities;
using Exam.Repositories.Interfaces.Repositories;
using Exam.Services.Exceptions;
using Exam.Services.Mappers;
using Exam.Services.Models.Responses;
using Exam.Services.Models.Responses.Subjects;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Exam.Services.Features.Subjects.Queries.GetSubjectById;

public record GetSubjectByIdQuery(Guid Id) : IRequest<BaseServiceResponse>;


public class GetSubjectByIdQueryValidator : AbstractValidator<GetSubjectByIdQuery>
{
    public GetSubjectByIdQueryValidator()
    {
    }
}

public class GetSubjectByIdQueryHandler : IRequestHandler<GetSubjectByIdQuery, BaseServiceResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetSubjectByIdQueryHandler> _logger;

    public GetSubjectByIdQueryHandler(IUnitOfWork unitOfWork, ILogger<GetSubjectByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BaseServiceResponse> Handle(GetSubjectByIdQuery request, CancellationToken cancellationToken)
    {
        var subject = await _unitOfWork.GetRepository<Subject>()
            .Query()
            .AsNoTracking()
            .Include(s => s.Semester)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (subject == null)
        {
            _logger.LogError("Failed to retrieve subject with ID: {Id}", request.Id);
            throw new NotFoundException("Không tìm thấy môn học!");
        }

        _logger.LogInformation("Subject retrieved successfully with ID: {Id}", subject.Id);
        return new DataServiceResponse<SubjectResponse>()
        {
            Success = true,
            Message = "Lấy môn học thành công!",
            Data = subject.ToSubjectResponse()
        };
    }
}
