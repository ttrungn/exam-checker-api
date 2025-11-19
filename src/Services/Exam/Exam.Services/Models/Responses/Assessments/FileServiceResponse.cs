namespace Exam.Services.Models.Responses.Assessments;

public class FileServiceResponse
{
    public byte[] FileContent { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = "application/octet-stream";
    public string FileName { get; set; } = string.Empty;
}

