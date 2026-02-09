using ArticleService.Api.Endpoints;
using ArticleService.Application.Interfaces;
using ArticleService.Application.Services;
using ArticleService.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Add Infrastructure layer (DbContext, Repositories, ShardResolver)
builder.Services.AddInfrastructure();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// Map API endpoints
app.MapArticleEndpoints();

app.Run();
