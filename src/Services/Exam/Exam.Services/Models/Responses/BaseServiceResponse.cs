namespace Exam.Services.Models.Responses;

public class BaseServiceResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
}
