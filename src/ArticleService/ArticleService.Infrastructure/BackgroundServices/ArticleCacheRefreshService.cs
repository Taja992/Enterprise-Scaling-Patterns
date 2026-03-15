using ArticleService.Application.DTOs.Articles;
using ArticleService.Application.Interfaces;
using ArticleService.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ArticleService.Infrastructure.BackgroundServices;

public class ArticleCacheRefreshService : BackgroundService
{
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan ArticleWindow = TimeSpan.FromDays(14);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IArticleCache _cache;
    private readonly ILogger<ArticleCacheRefreshService> _logger;

    public ArticleCacheRefreshService(
        IServiceScopeFactory scopeFactory,
        IArticleCache cache,
        ILogger<ArticleCacheRefreshService> logger
    )
    {
        _scopeFactory = scopeFactory;
        _cache = cache;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "ArticleCacheRefreshService starting — will refresh every {Interval} minutes",
            RefreshInterval.TotalMinutes
        );

        await RefreshAllShardsAsync(stoppingToken);
        
        using var timer = new PeriodicTimer(RefreshInterval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RefreshAllShardsAsync(stoppingToken);
        }
    }

    private async Task RefreshAllShardsAsync(CancellationToken ct)
    {
        var since = DateTime.UtcNow.Subtract(ArticleWindow);
        _logger.LogInformation(
            "Starting cache refresh — fetching articles since {Since:yyyy-MM-dd}",
            since
        );

        foreach (var continent in Enum.GetValues<Continent>())
        {
            if (ct.IsCancellationRequested) break;

            try
            {
                await RefreshShardAsync(continent.ToString(), since, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to refresh cache for shard {Continent}",
                    continent
                );
            }
        }

        _logger.LogInformation("Cache refresh complete");
    }

    private async Task RefreshShardAsync(string continent, DateTime since, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IArticleRepository>();

        var articles = await repository.GetRecentArticlesAsync(continent, since);

        if (articles.Count == 0)
        {
            _logger.LogDebug("No recent articles found for shard {Continent}", continent);
            return;
        }

        var responses = articles
            .Select(a => new ArticleResponse(
                a.Id,
                a.Title,
                a.Content,
                a.Continent,
                a.PublishedAt,
                a.PublisherId
            ))
            .ToList();

        await _cache.SetBatchAsync(responses);

        _logger.LogInformation(
            "Refreshed {Count} article(s) into cache for shard {Continent}",
            responses.Count,
            continent
        );
    }
}