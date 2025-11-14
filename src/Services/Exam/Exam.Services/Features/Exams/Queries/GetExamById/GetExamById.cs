using Exam.Repositories.Interfaces.Repositories;
using Exam.Services.Exceptions;
using Exam.Services.Mappers;
using Exam.Services.Models.Responses;
using Exam.Services.Models.Responses.Exams;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Exam.Services.Features.Exams.Queries.GetExamById;

public record GetExamByIdQuery(Guid Id) : IRequest<BaseServiceResponse>;


public class GetExamByIdQueryValidator : AbstractValidator<GetExamByIdQuery>
{
    public GetExamByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Vui lòng nhập ID kỳ thi!");
    }
}

public class GetExamByIdQueryHandler : IRequestHandler<GetExamByIdQuery, BaseServiceResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetExamByIdQueryHandler> _logger;

    public GetExamByIdQueryHandler(IUnitOfWork unitOfWork, ILogger<GetExamByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BaseServiceResponse> Handle(GetExamByIdQuery request, CancellationToken cancellationToken)
    {
        var exam = await _unitOfWork.GetRepository<Domain.Entities.Exam>()
            .Query()
            .AsNoTracking()
            .Include(e => e.Semester)
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
        if (exam == null)
        {
            _logger.LogError("Failed to retrieve exam with ID: {Id}", request.Id);
            throw new NotFoundException("Khong tìm thấy kỳ thi!");
        }

        _logger.LogInformation("Exam retrieved successfully with ID: {Id}", exam.Id);
        return new DataServiceResponse<ExamResponse>()
        {
            Success = true,
            Message = "Lấy kỳ thi thành công!",
            Data = exam.ToExamResponse()
        };
    }
}
