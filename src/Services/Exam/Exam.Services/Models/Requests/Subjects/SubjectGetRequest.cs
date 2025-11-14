namespace Exam.Services.Models.Requests.Subjects;

public class SubjectGetRequest
{
    public string? Name { get; init; }
    public string? Code { get; init; }
    public bool? IsActive { get; init; }
    public int? PageIndex { get; init; }
    public int? PageSize { get; init; }
}
