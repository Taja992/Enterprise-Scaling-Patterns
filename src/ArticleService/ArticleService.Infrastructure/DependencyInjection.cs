using ArticleService.Application.Interfaces;
using ArticleService.Application.Services;
using ArticleService.Infrastructure.BackgroundServices;
using ArticleService.Infrastructure.Caching;
using ArticleService.Infrastructure.Repositories;
using ArticleService.Infrastructure.Sharding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace ArticleService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // ── Database sharding ─────────────────────────────────────────────────
        services.AddSingleton<IShardResolver, ShardResolver>();
        services.AddScoped<IArticleRepository, ArticleRepository>();
        services.AddScoped<IArticleAppService, ArticleAppService>();

        // ── Redis ─────────────────────────────────────────────────────────────
        // IConnectionMultiplexer is thread-safe and expensive to create; always Singleton.
        // abortConnect=false: do not throw at startup if Redis is temporarily unavailable —
        // StackExchange.Redis will reconnect automatically in the background.
        var redisConnectionString = configuration["Redis:ConnectionString"] ?? "localhost:6379";

        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect($"{redisConnectionString},abortConnect=false")
        );

        // ── Article cache ─────────────────────────────────────────────────────
        // Singleton: the IConnectionMultiplexer is singleton, and the cache itself
        // holds no per-request state.
        services.AddSingleton<IArticleCache, ArticleRedisCache>();

        // ── Background refresh service ────────────────────────────────────────
        services.AddHostedService<ArticleCacheRefreshService>();

        return services;
    }
}
