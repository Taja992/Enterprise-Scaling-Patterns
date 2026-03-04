using HappyHeadlines.Observability;
using ProfanityService.Api.Endpoints;
using ProfanityService.Infrastructure;
using ProfanityService.Infrastructure.Persistence;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddHappyHeadlinesObservability("profanity-service");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProfanityDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.UseHappyHeadlinesObservability();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapProfanityEndpoints();

app.Run();
