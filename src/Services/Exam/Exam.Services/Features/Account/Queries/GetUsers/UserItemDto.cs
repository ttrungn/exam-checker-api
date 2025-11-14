namespace Exam.Services.Features.Account.Queries.GetUsers;

public class UserItemDto
{
    public string? Id { get; set; }
    public string? Email { get; set; } = null!;
    public string? UserPrincipalName { get; set; }
    public string? DisplayName { get; set; }
    public string? GivenName { get; set; }
    public string? Surname { get; set; }
    public string? JobTitle { get; set; }
    public List<string>? Roles { get; set; }
}
