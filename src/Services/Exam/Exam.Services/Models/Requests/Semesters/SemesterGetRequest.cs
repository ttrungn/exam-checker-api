namespace Exam.Services.Models.Requests.Semesters;

public class SemesterGetRequest
{
    public string? Name { get; init; }
    public bool? IsActive { get; init; }
    public int? PageIndex { get; init; }
    public int? PageSize { get; init; }
}
