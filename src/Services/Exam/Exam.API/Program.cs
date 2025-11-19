using Asp.Versioning.ApiExplorer;
using Exam.API;
using Exam.API.Hubs;
using Exam.Repositories;
using Exam.Repositories.Repositories.Contexts;
using Exam.Services;

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

    // await app.InitialiseDatabaseAsync();
}

app.UseExceptionHandler();
app.UseRouting();
app.UseCors("ExamCheckerCors");
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger/index.html", false);
    return Task.CompletedTask;
});
app.MapHub<AccountNotificationsHub>("/hubs/account-notifications");
app.MapControllers();

app.Run();
