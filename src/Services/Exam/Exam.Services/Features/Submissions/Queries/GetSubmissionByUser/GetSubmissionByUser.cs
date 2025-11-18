using Exam.Domain.Enums;
using Exam.Repositories.Interfaces.Repositories;
using Exam.Services.Exceptions;
using Exam.Services.Mappers;
using Exam.Services.Models.Responses;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Exam.Services.Features.Submissions.Queries.GetSubmissionByUser;

public record GetSubmissionByUserQuery : IRequest<DataServiceResponse<GetSubmissionsByUserDto>>
{
    public Guid UserId { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int IndexFrom { get; set; } = 1;
    public string? ExamCode { get; init; }
    public string? SubjectCode { get; init; }
    public SubmissionStatus? Status { get; init; }
    public string? SubmissionName { get; init; }
    public AssessmentStatus? AssessmentStatus { get; init; }
    public UserRole Role { get; init; } = UserRole.Examiner;
}

public enum UserRole
{
    Examiner,
    Moderator
}

public class GetSubmissionByUserValidator : AbstractValidator<GetSubmissionByUserQuery>
{
    public GetSubmissionByUserValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        RuleFor(x => x.IndexFrom)
            .GreaterThanOrEqualTo(0).WithMessage("IndexFrom must be at least 0.");

        RuleFor(x => x.PageIndex)
            .GreaterThanOrEqualTo(1).WithMessage("PageIndex must be at least 1.")
            .GreaterThanOrEqualTo(x => x.IndexFrom).WithMessage("PageIndex must be >= IndexFrom.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");
    }
}

public class GetSubmissionByUserHandler
    : IRequestHandler<GetSubmissionByUserQuery, DataServiceResponse<GetSubmissionsByUserDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetSubmissionByUserHandler> _logger;

    public GetSubmissionByUserHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetSubmissionByUserHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<DataServiceResponse<GetSubmissionsByUserDto>> Handle(
        GetSubmissionByUserQuery request,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "GetSubmissionByUser invoked. UserId={UserId}, Role={Role}, ExamCode={ExamCode}, SubjectCode={SubjectCode}, Status={Status}, SubmissionName={SubmissionName}, AssessmentStatus={AssessmentStatus}, Page=({IndexFrom},{PageIndex},{PageSize})",
            request.UserId,
            request.Role,
            request.ExamCode,
            request.SubjectCode,
            request.Status,
            request.SubmissionName,
            request.AssessmentStatus,
            request.IndexFrom,
            request.PageIndex,
            request.PageSize);

        try
        {
            var repository = _unitOfWork.GetRepository<Exam.Domain.Entities.Submission>();

            var query = repository.Query()
                .Include(s => s.ExamSubject)
                    .ThenInclude(es => es.Exam)
                .Include(s => s.ExamSubject)
                    .ThenInclude(es => es.Subject)
                .Include(s => s.Assessments)
                .AsNoTracking();

            if (request.Role == UserRole.Examiner)
            {
                query = query.Where(s => s.ExaminerId == request.UserId);
            }
            else if (request.Role == UserRole.Moderator)
            {
                query = query.Where(s => s.ModeratorId == request.UserId);
            }

            if (!string.IsNullOrWhiteSpace(request.ExamCode))
            {
                query = query.Where(s => s.ExamSubject != null && s.ExamSubject.Exam.Code.Contains(request.ExamCode));
            }

            if (!string.IsNullOrWhiteSpace(request.SubjectCode))
            {
                query = query.Where(s => s.ExamSubject != null && s.ExamSubject.Subject.Code.Contains(request.SubjectCode));
            }

            if (request.Status.HasValue)
            {
                query = query.Where(s => s.Status == request.Status.Value);
            }

            // Filter by SubmissionName (search in Assessments)
            if (!string.IsNullOrWhiteSpace(request.SubmissionName))
            {
                query = query.Where(s => s.Assessments.Any(a =>
                    a.SubmissionName != null && a.SubmissionName.Contains(request.SubmissionName)));
            }

            // Filter by AssessmentStatus
            if (request.AssessmentStatus.HasValue)
            {
                query = query.Where(s => s.Assessments.Any(a => a.Status == request.AssessmentStatus.Value));
            }

            query = query.OrderByDescending(s => s.CreatedAt);

            var totalCount = query.Count();

            var submissions = await query
                .Skip((request.PageIndex - request.IndexFrom) * request.PageSize)
                .Take(request.PageSize)
                .Select(s => s.ToUserSubmissionDto(request.UserId))
                .ToListAsync(ct);

            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            var pagedSubmissions = new GetSubmissionsByUserDto(submissions, request.PageIndex, request.PageSize, request.IndexFrom);

            // Override lại các giá trị pagination đã tính từ database
            pagedSubmissions.TotalCount = totalCount;
            pagedSubmissions.TotalPages = totalPages;

            _logger.LogInformation(
                "GetSubmissionByUser success: Retrieved {Count} submissions out of {Total} for UserId={UserId}, Role={Role}",
                submissions.Count,
                totalCount,
                request.UserId,
                request.Role);

            return new DataServiceResponse<GetSubmissionsByUserDto>()
            {
                Success = true,
                Message = $"Lấy danh sách submissions của {request.Role} thành công",
                Data = pagedSubmissions
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetSubmissionByUser failed for UserId={UserId}", request.UserId);
            throw new ServiceUnavailableException("Đã có lỗi hệ thống xảy ra, vui lòng liên hệ admin để được hỗ trợ!");
        }
    }
}
