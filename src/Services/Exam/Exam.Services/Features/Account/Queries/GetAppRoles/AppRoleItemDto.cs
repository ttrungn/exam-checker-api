namespace Exam.Services.Features.Account.Queries.GetAppRoles;

public class AppRoleItemDto
{
    public Guid? Id { get; set; }
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public string? Value { get; set; }
}
