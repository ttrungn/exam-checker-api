using System.Text.Json.Serialization;
using Exam.Services.Models.Responses.Exams;
using Exam.Services.Models.Responses.Subjects;

namespace Exam.Services.Models.Responses.ExamSubjects;

public class ExamSubjectResponse
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
    public Guid SubjectId { get; set; }
    
    public string ExamCode { get; set; } = string.Empty;

    public string SubjectCode { get; set; } = string.Empty;

    public string? ScoreStructure { get; set; }
    public string? ViolationStructure { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
    
   
}
