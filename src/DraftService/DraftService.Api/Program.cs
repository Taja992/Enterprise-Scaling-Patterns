using DraftService.Api.Endpoints;
using DraftService.Infrastructure;
using DraftService.Infrastructure.Persistence;
using HappyHeadlines.Observability;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ── Observability (Serilog + OpenTelemetry) ───────────────────────────────────
builder.AddHappyHeadlinesObservability("draft-service");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// ── Auto-create schema on first run ──────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DraftDbContext>();
    await db.Database.EnsureCreatedAsync();
}

// ── Middleware ─────────────────────────────────────────────────────────────────
app.UseHappyHeadlinesObservability(); // CorrelationId + Serilog request logging

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapDraftEndpoints();

app.Run();