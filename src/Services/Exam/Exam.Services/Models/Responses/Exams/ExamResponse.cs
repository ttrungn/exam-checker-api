using System.Text.Json.Serialization;
using Exam.Services.Models.Responses.Semesters;

namespace Exam.Services.Models.Responses.Exams;

public class ExamResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public SemesterResponse? Semester { get; set; } = null!;
}
