using ArticleService.Api.Endpoints;
using ArticleService.Infrastructure;
using HappyHeadlines.Observability;
using ArticleService.Infrastructure.Sharding;
using Prometheus;
using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddHappyHeadlinesObservability("article-service");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var resolver = scope.ServiceProvider.GetRequiredService<IShardResolver>();

    var continents = new[]
    {
        "Africa",
        "Antarctica",
        "Asia",
        "Europe",
        "NorthAmerica",
        "Oceania",
        "SouthAmerica",
        "Global"
    };

    foreach (var continent in continents)
    {
        var retries = 10;

        while (retries > 0)
        {
            try
            {
                var db = resolver.GetDbContext(continent);

                db.Database.EnsureCreated();

                Console.WriteLine($"Database ensured for shard: {continent}");
                break;
            }
            catch (Exception ex)
            {
                retries--;

                Console.WriteLine($"Waiting for database {continent}... {ex.Message}");

                Thread.Sleep(2000);

                if (retries == 0)
                    Console.WriteLine($"Database initialization failed for {continent}");
            }
        }
    }
}

app.UseHappyHeadlinesObservability();


    app.MapOpenApi();
    app.MapScalarApiReference();


app.UseHttpsRedirection();

app.MapArticleEndpoints();
app.MapMetrics();

app.Run();