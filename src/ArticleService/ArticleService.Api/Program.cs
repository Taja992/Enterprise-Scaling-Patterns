using ArticleService.Api.Endpoints;
using ArticleService.Infrastructure;
using HappyHeadlines.Observability;
using Prometheus;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddHappyHeadlinesObservability("article-service");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseHappyHeadlinesObservability();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapArticleEndpoints();
app.MapMetrics();

app.Run();