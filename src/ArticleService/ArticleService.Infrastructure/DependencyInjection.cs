using ArticleService.Application.Interfaces;
using ArticleService.Application.Services;
using ArticleService.Infrastructure.Repositories;
using ArticleService.Infrastructure.Sharding;
using Microsoft.Extensions.DependencyInjection;

namespace ArticleService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IShardResolver, ShardResolver>();
        services.AddScoped<IArticleRepository, ArticleRepository>();

        // Add Application services
        services.AddScoped<IArticleAppService, ArticleAppService>();

        return services;
    }
}
