using CommentService.Api.Endpoints;
using CommentService.Infrastructure;
using CommentService.Infrastructure.Persistence;
using HappyHeadlines.Observability;
using Prometheus;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddHappyHeadlinesObservability("comment-service");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CommentDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.UseHappyHeadlinesObservability();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapCommentEndpoints();

app.MapMetrics(); // Prometheus metrics endpoint at /metrics

app.Run();
