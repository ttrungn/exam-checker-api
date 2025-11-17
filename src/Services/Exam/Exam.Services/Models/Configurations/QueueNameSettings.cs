using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exam.Services.Models.Configurations;

public class QueueStorageSettings
{
    public string? ConnectionString { get; set; }
    public QueueNameSettings QueueNames { get; set; } = new();
}
public class QueueNameSettings
{
    public string? CompilationCheck { get; set; }
}
