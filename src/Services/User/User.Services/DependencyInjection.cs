using System.Reflection;
using Azure.Identity;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Graph;
using User.Services.Behaviours;

namespace User.Services;

public static class DependencyInjection
{
    public static void AddBusinessServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
        });
        builder.Services.AddSingleton(sp =>
        {
            var cfg = builder.Configuration.GetSection("AzureAd");
            var tenantId = cfg["TenantId"];
            var clientId = cfg["ClientId"];
            var clientSecret = cfg["ClientSecret"];
            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            return new GraphServiceClient(credential, ["https://graph.microsoft.com/.default"]);
        });
    }
}
