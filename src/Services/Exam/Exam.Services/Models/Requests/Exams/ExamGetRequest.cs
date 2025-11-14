namespace Exam.Services.Models.Requests.Exams;

public class ExamGetRequest
{
    public string? Code { get; set; }
    public bool? IsActive { get; set; }
    public int? PageIndex { get; set; }
    public int? PageSize { get; set; }
}
