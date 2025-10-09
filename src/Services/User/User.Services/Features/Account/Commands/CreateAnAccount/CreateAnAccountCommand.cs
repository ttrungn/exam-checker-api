using FluentValidation;
using MediatR;
using User.Services.Models.Responses;

namespace User.Services.Features.Account.Commands.CreateAnAccount;

public static class AccountConstants
{
    public static readonly string[] AgeGroups = ["Minor", "NotAdult", "Adult"];
    public static readonly string[] ConsentProvidedForMinors = ["Granted", "Denied", "NotRequired"];
}

public class CreateAnAccountCommand : IRequest<BaseServiceResponse>
{
    public string Email { get; set; } = null!;
    public string? DisplayName { get; set; }
    public string InitialPassword { get; set; } = null!;
    public string? GivenName { get; set; }
    public string? Surname { get; set; }
    public string? JobTitle { get; set; }
    public string? CompanyName { get; set; }
    public string? Department { get; set; }
    public string? EmployeeId { get; set; }
    public string? EmployeeType { get; set; }
    public DateTimeOffset? EmployeeHireDate { get; set; }
    public string? OfficeLocation { get; set; }
    public string? ManagerUserId { get; set; }
    public string? StreetAddress { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public List<string>? BusinessPhones { get; set; }
    public string? MobilePhone { get; set; }
    public string? FaxNumber { get; set; }
    public List<string>? OtherMails { get; set; }
    public string? AgeGroup { get; set; }
    public string? ConsentProvidedForMinor { get; set; }
    public string? UsageLocation { get; set; }
}

public class CreateAnAccountCommandValidator : AbstractValidator<CreateAnAccountCommand>
{
    public CreateAnAccountCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email address format.");
        RuleFor(x => x.DisplayName)
            .MaximumLength(256)
            .When(x => !string.IsNullOrWhiteSpace(x.DisplayName));
        RuleFor(x => x.InitialPassword)
            .NotEmpty().WithMessage("InitialPassword is required.")
            .MinimumLength(8).WithMessage("InitialPassword must be at least 8 characters.");
        RuleFor(x => x.GivenName).MaximumLength(128).When(x => !string.IsNullOrWhiteSpace(x.GivenName));
        RuleFor(x => x.Surname).MaximumLength(128).When(x => !string.IsNullOrWhiteSpace(x.Surname));
        RuleFor(x => x.JobTitle).MaximumLength(128).When(x => !string.IsNullOrWhiteSpace(x.JobTitle));
        RuleFor(x => x.CompanyName).MaximumLength(256).When(x => !string.IsNullOrWhiteSpace(x.CompanyName));
        RuleFor(x => x.Department).MaximumLength(128).When(x => !string.IsNullOrWhiteSpace(x.Department));
        RuleFor(x => x.EmployeeId).MaximumLength(64).When(x => !string.IsNullOrWhiteSpace(x.EmployeeId));
        RuleFor(x => x.EmployeeType).MaximumLength(64).When(x => !string.IsNullOrWhiteSpace(x.EmployeeType));
        RuleFor(x => x.OfficeLocation).MaximumLength(64).When(x => !string.IsNullOrWhiteSpace(x.OfficeLocation));
        RuleFor(x => x.UsageLocation)
            .Matches("^[A-Z]{2}$").WithMessage("UsageLocation must be ISO 2-letter country code (e.g., 'VN').")
            .When(x => !string.IsNullOrWhiteSpace(x.UsageLocation));
        RuleFor(x => x.AgeGroup)
            .Must(v => AccountConstants.AgeGroups.Contains(v!))
            .When(x => !string.IsNullOrWhiteSpace(x.AgeGroup))
            .WithMessage("AgeGroup must be one of: minor, notAdult, adult.");
        RuleFor(x => x.ConsentProvidedForMinor)
            .Must(v => AccountConstants.ConsentProvidedForMinors.Contains(v!))
            .When(x => !string.IsNullOrWhiteSpace(x.ConsentProvidedForMinor))
            .WithMessage("ConsentProvidedForMinor must be one of: Granted, Denied, NotRequired.");
    }
}
