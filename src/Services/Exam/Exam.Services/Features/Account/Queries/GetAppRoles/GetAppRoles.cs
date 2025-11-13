using Exam.Services.Exceptions;
using Exam.Services.Interfaces.Services;
using Exam.Services.Models.Responses;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Exam.Services.Features.Account.Queries.GetAppRoles;

public class GetAppRolesQuery : IRequest<DataServiceResponse<GetAppRolesDto>>
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int IndexFrom { get; set; } = 1;
}

public class GetAppRolesQueryValidator : AbstractValidator<GetAppRolesQuery>
{
    public GetAppRolesQueryValidator()
    {
        RuleFor(x => x.IndexFrom)
            .GreaterThanOrEqualTo(0)
            .WithMessage("IndexFrom must be at least 0.");

        RuleFor(x => x.PageIndex)
            .GreaterThanOrEqualTo(0)
            .WithMessage("PageIndex must be at least 1.")
            .GreaterThanOrEqualTo(x => x.IndexFrom)
            .WithMessage("PageIndex must be greater than or equal to IndexFrom.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100.");
    }
}

public class GetAppRolesHandler : IRequestHandler<GetAppRolesQuery, DataServiceResponse<GetAppRolesDto>>
{
    private readonly IConfiguration _configuration;
    private readonly IGraphClientService _graphClientService;
    private readonly ILogger<GetAppRolesHandler> _logger;

    public GetAppRolesHandler(
        IConfiguration configuration,
        IGraphClientService graphClientService,
        ILogger<GetAppRolesHandler> logger)
    {
        _configuration = configuration;
        _graphClientService = graphClientService;
        _logger = logger;
    }

    public async Task<DataServiceResponse<GetAppRolesDto>> Handle(
        GetAppRolesQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching application roles from Microsoft Graph.");
        var clientId = _configuration["AzureAD:ClientId"];
        if (string.IsNullOrWhiteSpace(clientId))
        {
            _logger.LogError("AzureAD:ClientId is not configured.");
            throw new ServiceUnavailableException("Đã có lỗi hệ thống xảy ra, vui lòng liên hệ admin để được hỗ trợ!");
        }

        var appRoles = await _graphClientService.GetAppRolesAsync(clientId, cancellationToken);

        var roles = appRoles
            .Select(role => new AppRoleItemDto
            {
                Id = role.Id, DisplayName = role.DisplayName, Value = role.Value, Description = role.Description
            })
            .ToList();

        var pagedRoles = new GetAppRolesDto(
            roles,
            request.PageIndex,
            request.PageSize,
            request.IndexFrom
        );

        _logger.LogInformation("Fetched {Count} roles (page {PageIndex}/{TotalPages}) from GraphClientService.",
            pagedRoles.TotalCurrentCount,
            pagedRoles.PageIndex,
            pagedRoles.TotalPages
        );

        return new DataServiceResponse<GetAppRolesDto>
        {
            Success = true, Message = "Application roles fetched successfully.", Data = pagedRoles
        };
    }
}
