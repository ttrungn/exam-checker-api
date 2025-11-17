using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Domain.Models.Configurations;

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
        services.Configure<BlobSettings>(configuration.GetSection("Azure:BlobStorageSettings"));
        // Add BlobServiceClient
        services.AddSingleton(sp =>
        {
            var blobSettings = sp.GetRequiredService<IOptions<BlobSettings>>().Value;
            return new BlobServiceClient(blobSettings.ConnectionString);
        });

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
