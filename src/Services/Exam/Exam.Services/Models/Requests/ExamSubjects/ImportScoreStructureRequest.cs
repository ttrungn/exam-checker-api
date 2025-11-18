using Microsoft.AspNetCore.Http;

namespace Exam.Services.Models.Requests.ExamSubjects;

public class ImportScoreStructureRequest
{
    public IFormFile File { get; set; } = null!;
}
