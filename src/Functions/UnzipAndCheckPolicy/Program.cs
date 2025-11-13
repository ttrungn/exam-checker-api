using Azure.Storage.Blobs;
using Exam.Repositories.Interfaces.Repositories;
using Exam.Repositories.Repositories;
using Exam.Repositories.Repositories.Contexts;
using Exam.Services.Interfaces.Services;
using Exam.Services.Models.Configurations;
using Exam.Services.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
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

        services.AddSingleton(sp =>
        {
            var s = sp.GetRequiredService<IOptions<BlobSettings>>().Value;
            return new BlobServiceClient(s.ConnectionString);
        });

        services.AddScoped<IAzureBlobService, AzureBlobService>();
        services.AddScoped<ISubmissionService, SubmissionService>();
    })
    .Build();

await host.RunAsync();
