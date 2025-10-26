using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using User.Services.Models.Responses;

namespace User.Services.Features.Account.Commands.CreateAnAccount;

public class CreateAnAccountHandler : IRequestHandler<CreateAnAccountCommand, BaseServiceResponse>
{
    // TODO: Move this to IOptions<AppSettings> in production
    private const string TenantDomain = "entiti832004outlook.onmicrosoft.com"; // your tenant's verified domain
    private readonly GraphServiceClient _graphClient;
    private readonly ILogger<CreateAnAccountHandler> _logger;

    public CreateAnAccountHandler(GraphServiceClient graphClient, ILogger<CreateAnAccountHandler> logger)
    {
        _graphClient = graphClient;
        _logger = logger;
    }

    public async Task<BaseServiceResponse> Handle(CreateAnAccountCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating MEMBER user for email seed {Email}", request.Email);

        // Derive a safe mailNickname from the left part of the provided email
        var mailNickname = request.Email.Split('@')[0]
            .Replace(".", "_")
            .Replace("-", "_")
            .Replace("+", "_");

        // Build the UPN under your tenant domain
        var userPrincipalName = $"{mailNickname}@{TenantDomain}";

        var user = new Microsoft.Graph.Models.User
        {
            Mail = request.Email,
            AccountEnabled = true,
            UserType = "Member",
            DisplayName = request.DisplayName ?? $"{request.GivenName} {request.Surname}".Trim(),
            MailNickname = mailNickname,
            UserPrincipalName = userPrincipalName,
            GivenName = request.GivenName,
            Surname = request.Surname,
            JobTitle = request.JobTitle,
            CompanyName = request.CompanyName,
            Department = request.Department,
            EmployeeId = request.EmployeeId,
            EmployeeType = request.EmployeeType,
            EmployeeHireDate = request.EmployeeHireDate,
            OfficeLocation = request.OfficeLocation,
            StreetAddress = request.StreetAddress,
            City = request.City,
            State = request.State,
            PostalCode = request.PostalCode,
            Country = request.Country,
            BusinessPhones = request.BusinessPhones ?? [],
            MobilePhone = request.MobilePhone,
            FaxNumber = request.FaxNumber,
            OtherMails = request.OtherMails ?? [],
            AgeGroup = request.AgeGroup,
            ConsentProvidedForMinor = request.ConsentProvidedForMinor,
            UsageLocation = request.UsageLocation,
            PasswordProfile = new PasswordProfile
            {
                Password = request.InitialPassword, ForceChangePasswordNextSignIn = true
            }
        };

        // IMPORTANT:
        // - Do NOT set Identities[] for a regular workforce tenant Member user.
        // - Do NOT set 'mail' (read-only; Exchange populates it when licensed).
        var created = await _graphClient.Users.PostAsync(user, cancellationToken: cancellationToken);
        if (created == null || string.IsNullOrWhiteSpace(created.Id))
        {
            return new BaseServiceResponse { Success = false, Message = "User creation failed (Graph returned null)." };
        }

        if (string.IsNullOrWhiteSpace(request.ManagerUserId))
        {
            return new BaseServiceResponse
            {
                Success = true,
                Message = $"User created successfully. UPN={created.UserPrincipalName}, Id={created.Id}"
            };
        }

        var managerRef = new ReferenceUpdate
        {
            OdataId = $"https://graph.microsoft.com/v1.0/users/{request.ManagerUserId}"
        };
        await _graphClient.Users[created.Id].Manager.Ref
            .PutAsync(managerRef, cancellationToken: cancellationToken);

        return new BaseServiceResponse
        {
            Success = true, Message = $"User created successfully. UPN={created.UserPrincipalName}, Id={created.Id}"
        };
    }
}
