using System.Text.Json.Serialization;
using Exam.Services.Models.Responses.Semesters;

namespace Exam.Services.Models.Responses.Subjects;

public class SubjectResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public SemesterResponse? Semester { get; set; } = null!;
}
