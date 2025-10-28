namespace Exam.Services.Features.Account.Queries.GetUserProfile;

public class UserProfileDto
{
    public string? Id { get; set; }
    public List<ObjectIdentityDto> Identities { get; set; } = [];
    public string? GivenName { get; set; }
    public string? Surname { get; set; }
    public string? UserType { get; set; }
    public AuthorizationInfoDto? AuthorizationInfo { get; set; }
    public string? JobTitle { get; set; }
    public string? CompanyName { get; set; }
    public string? Department { get; set; }
    public string? EmployeeId { get; set; }
    public string? EmployeeType { get; set; }
    public DateTimeOffset? EmployeeHireDate { get; set; }
    public string? OfficeLocation { get; set; }
    public ManagerDto? Manager { get; set; }
    public string? StreetAddress { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public List<string> BusinessPhones { get; set; } = [];
    public string? MobilePhone { get; set; }
    public string? Email { get; set; }
    public List<string> OtherEmails { get; set; } = [];
    public string? FaxNumber { get; set; }
    public string? AgeGroup { get; set; }
    public string? ConsentProvidedForMinor { get; set; }
    public string? UsageLocation { get; set; }
    public List<AppRoleDto> Roles { get; set; } = [];
}
