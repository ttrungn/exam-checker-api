using Domain.Constants;
using Exam.Repositories.Repositories.Collections;
using Exam.Services.Exceptions;
using Exam.Services.Interfaces.Services;
using Exam.Services.Mappers;
using Exam.Services.Models.Responses;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;




namespace Exam.Services.Features.Account.Queries.GetExaminers;
public record GetExaminersQuery : IRequest<DataServiceResponse<GetExaminersDto>>
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int IndexFrom { get; set; } = 1;
    public string? Email { get; init; }
}

public class GetExaminersQueryValidator : AbstractValidator<GetExaminersQuery>
{
    public GetExaminersQueryValidator()
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

public class GetExaminersHandler
    : IRequestHandler<GetExaminersQuery, DataServiceResponse<GetExaminersDto>>
{
    private readonly IConfiguration _configuration;
    private readonly GraphServiceClient _graphClient;
    private readonly IGraphClientService _graphClientService;
    private readonly ILogger<GetExaminersHandler> _logger;

    public GetExaminersHandler(
        IConfiguration configuration,
        GraphServiceClient graphClient,
        ILogger<GetExaminersHandler> logger,
        IGraphClientService graphClientService)
    {
        _configuration = configuration;
        _graphClient = graphClient;
        _logger = logger;
        _graphClientService = graphClientService;
    }

    public async Task<DataServiceResponse<GetExaminersDto>> Handle(
        GetExaminersQuery request,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "GetExaminers invoked. EmailFilter={Email}, Page=({IndexFrom},{PageIndex},{PageSize})",
            request.Email,
            request.IndexFrom,
            request.PageIndex,
            request.PageSize);

        var clientId = _configuration["AzureAD:ClientId"];
        if (string.IsNullOrWhiteSpace(clientId))
        {
            _logger.LogWarning("Missing AzureAD:ClientId configuration.");
            throw new ServiceUnavailableException("Đã có lỗi hệ thống xảy ra, vui lòng liên hệ admin để được hỗ trợ!");
        }

        try
        {
            var appRoles = await _graphClientService.GetAppRolesAsync(clientId, ct);
            
            // Find the Examiner role ID
            var examinerRole = appRoles.FirstOrDefault(r => r.Value == Roles.Examiner);
            if (examinerRole?.Id == null)
            {
                _logger.LogWarning("Examiner role not found in app roles.");
                throw new ServiceUnavailableException("Không tìm thấy role Examiner trong hệ thống!");
            }

            var examinerRoleId = examinerRole.Id.ToString();

            var res = await _graphClient.Users.GetAsync(r =>
            {
                if (!string.IsNullOrEmpty(request.Email))
                {
                    r.QueryParameters.Search =
                        $"\"mail:{request.Email.Trim()}\" OR \"userPrincipalName:{request.Email.Trim()}\"";
                    r.QueryParameters.Count = true;
                    r.Headers.Add("ConsistencyLevel", "eventual");
                }

                r.QueryParameters.Select =
                [
                    "id", "displayName", "mail", "userPrincipalName",
                    "givenName", "surname", "jobTitle"
                ];
            }, ct);

            if (res?.Value == null)
            {
                _logger.LogError("Cannot get users");
                throw new ServiceUnavailableException(
                    "Đã có lỗi hệ thống xảy ra, vui lòng liên hệ admin để được hỗ trợ!");
            }

            var examiners = new List<ExaminerItemDto>();
            
            // Filter users by Examiner role
            foreach (var tempUser in res.Value)
            {
                var userRoleAssignments = await _graphClient.Users[tempUser.Id].AppRoleAssignments.GetAsync(
                    r =>
                    {
                        r.QueryParameters.Select = ["appRoleId"];
                    }, ct);

                var roles = userRoleAssignments?.Value;
                if (roles == null || roles.Count == 0)
                {
                    continue;
                }

                // Check if user has Examiner role
                var hasExaminerRole = roles.Any(role => 
                    role.AppRoleId?.ToString() == examinerRoleId);

                if (hasExaminerRole)
                {
                    examiners.Add(tempUser.ToExaminerItemDto());
                }
            }

            var pagedExaminers = new GetExaminersDto(examiners, request.PageIndex, request.PageSize, request.IndexFrom);

            return new DataServiceResponse<GetExaminersDto>()
            {
                Success = true,
                Message = "Lấy danh sách examiner thành công",
                Data = pagedExaminers
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetExaminers failed.");
            throw new ServiceUnavailableException("Đã có lỗi hệ thống xảy ra, vui lòng liên hệ admin để được hỗ trợ!");
        }
    }
}
