using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        // Configure HttpClient for calling Exam API
        services.AddHttpClient("ExamAPI", client =>
        {
            var apiBaseUrl = configuration["ExamAPI:BaseUrl"] 
                ?? throw new InvalidOperationException("ExamAPI:BaseUrl is not configured");
            client.BaseAddress = new Uri(apiBaseUrl);
            client.Timeout = TimeSpan.FromMinutes(5); // Long timeout for zip processing
        });
    })
    .Build();

await host.RunAsync();
