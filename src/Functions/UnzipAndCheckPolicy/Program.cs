using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Exam.Repositories.Interfaces.Repositories;
using Exam.Repositories.Repositories;
using Exam.Repositories.Repositories.Contexts;
using Exam.Services.Interfaces.Services;
using Exam.Services.Models.Configurations;
using Exam.Services.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUnitOfWork, UnitOfWork<ApplicationDbContext>>();
        services.Configure<BlobSettings>(configuration.GetSection("Azure:BlobStorageSettings"));

        // Add BlobServiceClient
        services.AddSingleton(sp =>
        {
            var blobSettings = sp.GetRequiredService<IOptions<BlobSettings>>().Value;
            return new BlobServiceClient(blobSettings.ConnectionString);
        });

        // Configure QueueStorageSettings
        services.Configure<QueueStorageSettings>(options =>
        {
            options.ConnectionString = configuration["Azure:QueueStorageSettings:ConnectionString"];
            options.QueueNames = new QueueNameSettings
            {
                CompilationCheck = configuration["Azure:QueueStorageSettings:QueueNames:CompilationCheck"]
            };
        });

        // Add QueueServiceClient
        services.AddSingleton(provider =>
        {
            var settings = provider.GetRequiredService<IOptions<QueueStorageSettings>>().Value;
            return new QueueServiceClient(settings.ConnectionString);
        });

        services.AddScoped<IAzureBlobService, AzureBlobService>();
        services.AddScoped<ISubmissionService, SubmissionService>();
        services.AddScoped<IViolationService, ViolationService>();
        services.AddScoped<IAzureQueueService, AzureQueueService>();
        
        // Configure HttpClient for calling Exam API
        services.AddHttpClient("ExamAPI", client =>
        {
            var apiBaseUrl = configuration["ExamAPI:BaseUrl"] 
                ?? throw new InvalidOperationException("ExamAPI:BaseUrl is not configured");
            client.BaseAddress = new Uri(apiBaseUrl);
            client.Timeout = TimeSpan.FromMinutes(5); // Long timeout for zip processing
        });
        
        // Add default HttpClient factory
        services.AddHttpClient();
    })
    .Build();

await host.RunAsync();
