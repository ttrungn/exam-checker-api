using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Exam.Services.Interfaces.Services;
using Exam.Services.Models.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Exam.Services.Services;
public class AzureQueueService : IAzureQueueService
{
    private readonly QueueServiceClient _client;
    private readonly QueueNameSettings _queueNames;
    private readonly ILogger<AzureQueueService> _logger;

    public AzureQueueService(
        QueueServiceClient client,
        IOptions<QueueStorageSettings> queueStorageOptions,
        ILogger<AzureQueueService> logger)
    {
        _client = client;
        _logger = logger;
        
        _logger.LogInformation($"QueueStorageSettings Value: {queueStorageOptions.Value != null}");
        _logger.LogInformation($"QueueNames: {queueStorageOptions.Value?.QueueNames != null}");
        
        _queueNames = queueStorageOptions.Value?.QueueNames ?? throw new InvalidOperationException("QueueNames configuration is null");
        
        _logger.LogInformation($"QueueNames initialized. CompilationCheck value: {_queueNames.CompilationCheck ?? "NULL"}");
    }

    public async Task<bool> SendMessageAsync<T>(string queueKey, T message, CancellationToken ct = default)
    {
        _logger.LogInformation($"SendMessageAsync called with queueKey: {queueKey}");
        
        var property = typeof(QueueNameSettings).GetProperty(queueKey);
        if (property == null)
        {
            _logger.LogError($"Property '{queueKey}' not found on QueueNameSettings");
            throw new ArgumentException($"Property '{queueKey}' does not exist in QueueNameSettings class.");
        }

        var queueName = property.GetValue(_queueNames)?.ToString();
        _logger.LogInformation($"Queue name resolved: {queueName ?? "NULL"}");

        if (string.IsNullOrWhiteSpace(queueName))
            throw new ArgumentException($"Queue name for key '{queueKey}' is null or empty in configuration.");

        var queueClient = _client.GetQueueClient(queueName);
        await queueClient.CreateIfNotExistsAsync(cancellationToken: ct);

        var payload = JsonSerializer.Serialize(message);
        var base64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payload));
        await queueClient.SendMessageAsync(base64, cancellationToken: ct);

        return true;
    }
}
