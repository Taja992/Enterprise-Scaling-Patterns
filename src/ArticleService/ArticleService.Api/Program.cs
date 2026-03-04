using ArticleService.Api.Endpoints;
using ArticleService.Infrastructure;
using HappyHeadlines.Observability;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ── Observability (Serilog + OpenTelemetry) ───────────────────────────────────
builder.AddHappyHeadlinesObservability("article-service");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// ── Middleware ────────────────────────────────────────────────────────────────
app.UseHappyHeadlinesObservability(); // CorrelationId + Serilog request logging

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapArticleEndpoints();
app.MapCommentEndpoints();
app.MapProfanityEndpoints();

app.Run();
