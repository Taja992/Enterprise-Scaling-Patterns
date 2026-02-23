using ArticleService.Application.Common;
using ArticleService.Application.Interfaces;
using ArticleService.Application.Services;
using ArticleService.Infrastructure.Persistence;
using ArticleService.Infrastructure.Repositories;
using ArticleService.Infrastructure.Sharding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArticleService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Articles — sharded by continent (existing)
        services.AddSingleton<IShardResolver, ShardResolver>();
        services.AddScoped<IArticleRepository, ArticleRepository>();
        services.AddScoped<IArticleAppService, ArticleAppService>();

        // Comments swimlane — isolated DB
        services.AddDbContext<CommentDbContext>(o =>
            o.UseNpgsql(configuration.GetConnectionString("Comments"))
        );
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<ICommentService, CommentService>();

        // Profanity swimlane — isolated DB
        services.AddDbContext<ProfanityDbContext>(o =>
            o.UseNpgsql(configuration.GetConnectionString("Profanity"))
        );
        services.AddScoped<IProfanityRepository, ProfanityRepository>();
        services.AddScoped<IProfanityService, ProfanityService>();

        // Circuit breaker — Singleton so state persists across requests
        services.AddSingleton<CommentCircuitBreaker>();

        return services;
    }
}
