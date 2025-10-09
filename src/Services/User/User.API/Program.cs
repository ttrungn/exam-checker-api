using Asp.Versioning.ApiExplorer;
using User.API;
using User.Repositories;
using User.Repositories.Repositories.Contexts;
using User.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddRepositoryServices();
builder.AddBusinessServices();
builder.AddApiServices();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        foreach (var desc in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint(
                $"/swagger/{desc.GroupName}/swagger.json",
                $"My Application API {desc.GroupName.ToUpperInvariant()}" + (desc.IsDeprecated ? " (deprecated)" : "")
            );
        }
    });

    await app.InitialiseDatabaseAsync();
}

app.UseExceptionHandler();
app.UseRouting();
app.UseCors("AllowLocationHeader");
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger/index.html", false);
    return Task.CompletedTask;
});
app.MapControllers();

app.Run();
