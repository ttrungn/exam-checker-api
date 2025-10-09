using User.Repositories.Extensions;
using User.Repositories.Repositories.Contexts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;

namespace User.Repositories;

public static class DependencyInjection
{
    public static void AddRepositoryServices(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
        builder.Services.AddUnitOfWork<ApplicationDbContext>();
        builder.Services.AddScoped<ApplicationDbContextInitializer>();
    }
}
