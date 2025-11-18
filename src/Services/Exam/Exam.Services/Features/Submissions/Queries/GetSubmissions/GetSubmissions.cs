using Exam.Domain.Enums;
using Exam.Repositories.Interfaces.Repositories;
using Exam.Services.Exceptions;
using Exam.Services.Mappers;
using Exam.Services.Models.Responses;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;

namespace Exam.Services.Features.Submissions.Queries.GetSubmissions;

public record GetSubmissionsQuery : IRequest<DataServiceResponse<GetSubmissionsDto>>
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int IndexFrom { get; set; } = 1;
    public string? ExamCode { get; init; }
    public string? SubjectCode { get; init; }
    public SubmissionStatus? Status { get; init; }
    public string? ExaminerName { get; init; }
    public string? ModeratorName { get; init; }
    public string? SubmissionName { get; init; }
    public AssessmentStatus? AssessmentStatus { get; init; }
}

public class GetSubmissionsQueryValidator : AbstractValidator<GetSubmissionsQuery>
{
    public GetSubmissionsQueryValidator()
    {
        RuleFor(x => x.IndexFrom)
            .GreaterThanOrEqualTo(0).WithMessage("IndexFrom must be at least 0.");

        RuleFor(x => x.PageIndex)
            .GreaterThanOrEqualTo(1).WithMessage("PageIndex must be at least 1.")
            .GreaterThanOrEqualTo(x => x.IndexFrom).WithMessage("PageIndex must be >= IndexFrom.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");
    }
}

public class GetSubmissionsHandler
    : IRequestHandler<GetSubmissionsQuery, DataServiceResponse<GetSubmissionsDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetSubmissionsHandler> _logger;
    private readonly GraphServiceClient _graphClient;

    public GetSubmissionsHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetSubmissionsHandler> logger,
        GraphServiceClient graphClient)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _graphClient = graphClient;
    }

    public async Task<DataServiceResponse<GetSubmissionsDto>> Handle(
        GetSubmissionsQuery request,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "GetSubmissions invoked. ExamCode={ExamCode}, SubjectCode={SubjectCode}, Status={Status}, ExaminerName={ExaminerName}, ModeratorName={ModeratorName}, SubmissionName={SubmissionName}, AssessmentStatus={AssessmentStatus}, Page=({IndexFrom},{PageIndex},{PageSize})",
            request.ExamCode,
            request.SubjectCode,
            request.Status,
            request.ExaminerName,
            request.ModeratorName,
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

            // Filter by ExamCode
            if (!string.IsNullOrWhiteSpace(request.ExamCode))
            {
                query = query.Where(s => s.ExamSubject != null &&
                    s.ExamSubject.Exam != null && s.ExamSubject.Exam.Code.Contains(request.ExamCode));
            }

            // Filter by SubjectCode
            if (!string.IsNullOrWhiteSpace(request.SubjectCode))
            {
                query = query.Where(s => s.ExamSubject != null &&
                    s.ExamSubject.Subject != null && s.ExamSubject.Subject.Code.Contains(request.SubjectCode));
            }

            // Filter by Status
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

            // Filter by ExaminerEmail
            if (!string.IsNullOrWhiteSpace(request.ExaminerName))
            {
                try
                {
                    var examiners = await _graphClient.Users.GetAsync(r =>
                    {
                        r.QueryParameters.Search = $"\"mail:{request.ExaminerName}\" OR \"userPrincipalName:{request.ExaminerName}\"";
                        r.QueryParameters.Count = true;
                        r.QueryParameters.Select = ["id"];
                        r.Headers.Add("ConsistencyLevel", "eventual");
                    }, ct);

                    if (examiners?.Value != null && examiners.Value.Count > 0)
                    {
                        var examinerIds = examiners.Value.Select(u => Guid.Parse(u.Id!)).ToList();
                        query = query.Where(s => s.ExaminerId.HasValue && examinerIds.Contains(s.ExaminerId.Value));
                    }
                    else
                    {
                        // Không tìm thấy examiner nào, trả về empty
                        query = query.Where(s => false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to search examiner by name: {ExaminerName}", request.ExaminerName);
                }
            }

            // Filter by ModeratorName (search by email in Azure AD)
            if (!string.IsNullOrWhiteSpace(request.ModeratorName))
            {
                try
                {
                    var moderators = await _graphClient.Users.GetAsync(r =>
                    {
                        r.QueryParameters.Search = $"\"mail:{request.ModeratorName}\" OR \"userPrincipalName:{request.ModeratorName}\"";
                        r.QueryParameters.Count = true;
                        r.QueryParameters.Select = ["id"];
                        r.Headers.Add("ConsistencyLevel", "eventual");
                    }, ct);

                    if (moderators?.Value != null && moderators.Value.Count > 0)
                    {
                        var moderatorIds = moderators.Value.Select(u => Guid.Parse(u.Id!)).ToList();
                        query = query.Where(s => s.ModeratorId.HasValue && moderatorIds.Contains(s.ModeratorId.Value));
                    }
                    else
                    {
                        // Không tìm thấy moderator nào, trả về empty
                        query = query.Where(s => false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to search moderator by name: {ModeratorName}", request.ModeratorName);
                }
            }

            // Order by CreatedAt descending
            query = query.OrderByDescending(s => s.CreatedAt);

            // Get total count BEFORE pagination
            var totalCount = await query.CountAsync(ct);

            // Get submissions for current page
            var submissions = await query
                .Skip((request.PageIndex - request.IndexFrom) * request.PageSize)
                .Take(request.PageSize)
                .Select(s => s.ToManagerSubmissionDto())
                .ToListAsync(ct);

            // Lấy email của Examiner và Moderator từ Azure AD
            var userIds = new HashSet<Guid>();
            foreach (var submission in submissions)
            {
                if (submission.ExaminerId.HasValue)
                    userIds.Add(submission.ExaminerId.Value);
                if (submission.ModeratorId.HasValue)
                    userIds.Add(submission.ModeratorId.Value);
            }

            // Dictionary để cache email theo userId
            var userEmailCache = new Dictionary<Guid, string>();

            if (userIds.Any())
            {
                foreach (var userId in userIds)
                {
                    try
                    {
                        var user = await _graphClient.Users[userId.ToString()].GetAsync(r =>
                        {
                            r.QueryParameters.Select = ["mail", "userPrincipalName"];
                        }, ct);

                        if (user != null)
                        {
                            userEmailCache[userId] = user.Mail ?? user.UserPrincipalName ?? "";
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to get user info for {UserId}", userId);
                        userEmailCache[userId] = "";
                    }
                }
            }

            // Map email vào DTO
            foreach (var submission in submissions)
            {
                if (submission.ExaminerId.HasValue && userEmailCache.ContainsKey(submission.ExaminerId.Value))
                {
                    submission.ExaminerEmail = userEmailCache[submission.ExaminerId.Value];
                }

                if (submission.ModeratorId.HasValue && userEmailCache.ContainsKey(submission.ModeratorId.Value))
                {
                    submission.ModeratorEmail = userEmailCache[submission.ModeratorId.Value];
                }
            }

            // Tính toán pagination metadata
            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            // Tạo pagedSubmissions với constructor
            var pagedSubmissions = new GetSubmissionsDto(submissions, request.PageIndex, request.PageSize, request.IndexFrom);

            // Override lại các giá trị pagination đã tính từ database
            pagedSubmissions.TotalCount = totalCount;
            pagedSubmissions.TotalPages = totalPages;

            _logger.LogInformation("GetSubmissions success: Retrieved {Count} submissions out of {Total}", submissions.Count, totalCount);

            return new DataServiceResponse<GetSubmissionsDto>()
            {
                Success = true,
                Message = "Lấy danh sách submissions thành công",
                Data = pagedSubmissions
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetSubmissions failed.");
            throw new ServiceUnavailableException("Đã có lỗi hệ thống xảy ra, vui lòng liên hệ admin để được hỗ trợ!");
        }
    }
}
