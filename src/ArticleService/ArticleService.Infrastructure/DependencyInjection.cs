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
        services.AddSingleton<IShardResolver, ShardResolver>();
        services.AddScoped<IArticleRepository, ArticleRepository>();
        services.AddScoped<IArticleAppService, ArticleAppService>();
        
        var redisConnectionString =
            configuration["Redis:ConnectionString"] ?? "localhost:6379";

        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect($"{redisConnectionString},abortConnect=false")
        );
        
        services.AddSingleton<IArticleCache, ArticleRedisCache>();
        
        services.AddHostedService<ArticleCacheRefreshService>();

        return services;
    }
}