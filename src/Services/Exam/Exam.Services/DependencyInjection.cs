using System.Reflection;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Exam.Services.Behaviours;
using Exam.Services.Interfaces.Services;
using Exam.Services.Models.Configurations;
using Exam.Services.Services;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Graph;

namespace Exam.Services;

public static class DependencyInjection
{
    public static void AddBusinessServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Register Service
        builder.Services.AddScoped<ISemesterService, SemesterService>();
        builder.Services.AddScoped<IAzureBlobService, AzureBlobService>();
        builder.Services.AddScoped<IAzureQueueService, AzureQueueService>();
        builder.Services.AddScoped<IViolationService, ViolationService>();
        builder.Services.AddScoped<ISubmissionService, SubmissionService>();
        builder.Services.AddScoped<IGraphClientService, GraphClientService>();
        builder.Services.AddScoped<IExamSubjectService, ExamSubjectService>();  

        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
        });
        builder.Services.AddSingleton(_ =>
        {
            var cfg = builder.Configuration.GetSection("AzureAd");
            var tenantId = cfg["TenantId"];
            var clientId = cfg["ClientId"];
            var clientSecret = cfg["ClientSecret"];
            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            return new GraphServiceClient(credential, ["https://graph.microsoft.com/.default"]);
        });

        // Blob settings
        var blobSettings = builder.Configuration.GetSection("Azure:BlobStorageSettings");
        var blobConnectionString = blobSettings.GetValue<string>("ConnectionString");
        builder.Services.Configure<BlobSettings>(blobSettings);
        builder.Services.AddSingleton(new BlobServiceClient(blobConnectionString));

        //Queue Settings - Fix: Use Bind instead of Configure for complex objects
        var queueSettings = builder.Configuration.GetSection("Azure:QueueStorageSettings");
        var queueConnectionString = queueSettings.GetValue<string>("ConnectionString");
        
        var queueStorageSettings = new QueueStorageSettings();
        queueSettings.Bind(queueStorageSettings);
        builder.Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(queueStorageSettings));
        
        builder.Services.AddSingleton(new QueueServiceClient(queueConnectionString));
    }
}
