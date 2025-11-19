namespace Exam.Services.Models.Responses.Dashboard;

public class DashboardSummaryResponse
{
    public Guid ExamSubjectId { get; set; }
    public Guid ExamId { get; set; }
    public Guid SubjectId { get; set; }
    public string ExamCode { get; set; } = String.Empty;
    public string SubjectCode { get; set; } = String.Empty;
    public int TotalSubmissions { get; set; }
    public int Graded { get; set; }
    
    public int Reassigned { get; set; }
    
    public int Approved { get; set; }
    public int NotGraded { get; set; }
    public int Violated { get; set; }   
    public decimal ProgressPercent { get; set; }
}
