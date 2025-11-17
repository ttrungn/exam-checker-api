namespace UnzipAndCheckPolicy.Models;

public class CompilationCheckQueueMessage
{
    public Guid SubmissionId { get; set; }

    public string BlobUrl { get; set; } = null!;
}
