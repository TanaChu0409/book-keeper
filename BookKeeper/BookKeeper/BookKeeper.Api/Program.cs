using BookKeeper.Api;
using BookKeeper.Api.Endpoints;
using BookKeeper.Api.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddApiService()
    .AddMediaR()
    .AddErrorHandling()
    .AddDatabase()
    .AddObservability() 
    .AddApplicationServices()
    .AddAuthenticationService();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    await app.ApplyMigrationsAsync();
    await app.SeedInitialDataAsync();
}

app.UseHttpsRedirection();

app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();

await app.RunAsync();
