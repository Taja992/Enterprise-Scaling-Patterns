using ArticleService.Api.Endpoints;
using ArticleService.Infrastructure;
using HappyHeadlines.Observability;
using Prometheus;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ── Observability (Serilog + OpenTelemetry) ───────────────────────────────────
builder.AddHappyHeadlinesObservability("article-service");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// ── Middleware ────────────────────────────────────────────────────────────────
app.UseHappyHeadlinesObservability();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapArticleEndpoints();
app.MapMetrics(); // <-- exposes /metrics for Prometheus to scrape

app.Run();
