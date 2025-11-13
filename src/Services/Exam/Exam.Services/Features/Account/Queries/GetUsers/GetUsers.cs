using Exam.Services.Exceptions;
using Exam.Services.Interfaces.Services;
using Exam.Services.Mappers;
using Exam.Services.Models.Responses;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;

namespace Exam.Services.Features.Account.Queries.GetUsers;

public record GetUsersQuery : IRequest<DataServiceResponse<GetUsersDto>>
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int IndexFrom { get; set; } = 1;
    public List<string>? AppRoleIds { get; init; }
    public string? Email { get; init; }
}

public class GetUsersByAppRoleQueryValidator : AbstractValidator<GetUsersQuery>
{
    public GetUsersByAppRoleQueryValidator()
    {
        RuleFor(x => x.AppRoleIds);

        RuleFor(x => x.IndexFrom)
            .GreaterThanOrEqualTo(0).WithMessage("IndexFrom must be at least 0.");

        RuleFor(x => x.PageIndex)
            .GreaterThanOrEqualTo(1).WithMessage("PageIndex must be at least 1.")
            .GreaterThanOrEqualTo(x => x.IndexFrom).WithMessage("PageIndex must be >= IndexFrom.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");

        RuleFor(x => x.Email);
    }
}

public class GetUsersHandler
    : IRequestHandler<GetUsersQuery, DataServiceResponse<GetUsersDto>>
{
    private readonly IConfiguration _configuration;
    private readonly GraphServiceClient _graphClient;
    private readonly IGraphClientService _graphClientService;
    private readonly ILogger<GetUsersHandler> _logger;

    public GetUsersHandler(
        IConfiguration configuration,
        GraphServiceClient graphClient,
        ILogger<GetUsersHandler> logger,
        IGraphClientService graphClientService)
    {
        _configuration = configuration;
        _graphClient = graphClient;
        _logger = logger;
        _graphClientService = graphClientService;
    }

    public async Task<DataServiceResponse<GetUsersDto>> Handle(
        GetUsersQuery request,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "GetUsers invoked. RoleCount={RoleCount}, EmailFilter={Email}, Page=({IndexFrom},{PageIndex},{PageSize})",
            request.AppRoleIds?.Count ?? 0,
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

            var users = new List<UserItemDto>();
            if (request.AppRoleIds == null)
            {
                users.AddRange(res.Value.Select(x => x.ToUserItemDto(appRoles)));
            }
            else
            {
                var appRoleIdSet = new HashSet<string>(request.AppRoleIds);
                foreach (var tempUser in res.Value)
                {
                    var userRoleAssignments = await _graphClient.Users[tempUser.Id].AppRoleAssignments.GetAsync(
                        r =>
                        {
                            r.QueryParameters.Select = ["appRoleId"];
                        }, ct);

                    var roles = userRoleAssignments?.Value;
                    if (roles?.Count == 0)
                    {
                        continue;
                    }

                    foreach (var role in roles!)
                    {
                        var roleId = role.AppRoleId?.ToString();
                        if (!string.IsNullOrEmpty(roleId) && appRoleIdSet.Contains(roleId))
                        {
                            users.Add(tempUser.ToUserItemDto(appRoles));
                            break;
                        }
                    }
                }
            }

            var pagedUsers = new GetUsersDto(users, request.PageIndex, request.PageSize, request.IndexFrom);

            return new DataServiceResponse<GetUsersDto>()
            {
                Success = true, Message = "Lấy danh sách người dùng thành công", Data = pagedUsers
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetUsers failed.");
            throw new ServiceUnavailableException("Đã có lỗi hệ thống xảy ra, vui lòng liên hệ admin để được hỗ trợ!");
        }
    }
}
