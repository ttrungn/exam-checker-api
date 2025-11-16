using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exam.Services.Models.QueueMessages;
public class CompilationCheckQueueMessage
{
    public Guid SubmissionId { get; set; }

    public string BlobUrl { get; set; } = null!;
}
