using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exam.Services.Interfaces.Services;
public interface IAzureQueueService
{
    Task<bool> SendMessageAsync<T>(string queueName, T message, CancellationToken ct = default);

}
