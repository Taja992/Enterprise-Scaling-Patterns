using CommentService.Application.Common;
using CommentService.Application.Interfaces;
using CommentService.Application.Services;
using CommentService.Infrastructure.Caching;
using CommentService.Infrastructure.Http;
using CommentService.Infrastructure.Persistence;
using CommentService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace CommentService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // ── Database ──────────────────────────────────────────────────────────
        services.AddDbContext<CommentDbContext>(o =>
            o.UseNpgsql(configuration.GetConnectionString("CommentDb"))
        );

        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<ICommentAppService, CommentAppService>();

        // ── ProfanityService HTTP client ──────────────────────────────────────
        services.AddHttpClient<IProfanityServiceClient, ProfanityServiceClient>(client =>
        {
            var baseUrl =
                configuration["ProfanityService:BaseUrl"] ?? "http://profanity-api-1:8080";
            client.BaseAddress = new Uri(baseUrl);
        });

        // Singleton — circuit state must survive across requests
        services.AddSingleton<CommentCircuitBreaker>();

        // ── Redis ─────────────────────────────────────────────────────────────
        var redisConnectionString = configuration["Redis:ConnectionString"] ?? "localhost:6379";

        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect($"{redisConnectionString},abortConnect=false")
        );

        // Singleton: Redis connection is singleton, no per-request state.
        services.AddSingleton<ICommentCache, CommentRedisCache>();

        return services;
    }
}
