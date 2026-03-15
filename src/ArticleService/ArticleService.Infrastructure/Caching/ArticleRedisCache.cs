using System.Text.Json;
using ArticleService.Application.DTOs.Articles;
using ArticleService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Prometheus;
using StackExchange.Redis;

namespace ArticleService.Infrastructure.Caching;

public class ArticleRedisCache : IArticleCache
{
    private static readonly Counter CacheHits = Metrics.CreateCounter(
        "article_cache_hits_total",
        "Total number of ArticleCache hits.",
        new CounterConfiguration { LabelNames = new[] { "shard" } }
    );

    private static readonly Counter CacheMisses = Metrics.CreateCounter(
        "article_cache_misses_total",
        "Total number of ArticleCache misses.",
        new CounterConfiguration { LabelNames = new[] { "shard" } }
    );

    private static readonly TimeSpan Ttl = TimeSpan.FromHours(36);

    private readonly IDatabase _redis;
    private readonly ILogger<ArticleRedisCache> _logger;

    public ArticleRedisCache(IConnectionMultiplexer connection, ILogger<ArticleRedisCache> logger)
    {
        _redis = connection.GetDatabase();
        _logger = logger;
    }

    private static string BuildKey(Guid id, string continent) =>
        $"article:{continent.ToLowerInvariant()}:{id}";

    public async Task<ArticleResponse?> GetAsync(Guid id, string continent)
    {
        var key = BuildKey(id, continent);
        var value = await _redis.StringGetAsync(key);

        if (value.IsNullOrEmpty)
        {
            CacheMisses.WithLabels(continent.ToLowerInvariant()).Inc();
            _logger.LogDebug("Cache MISS for article {Id} in shard {Continent}", id, continent);
            return null;
        }

        CacheHits.WithLabels(continent.ToLowerInvariant()).Inc();
        _logger.LogDebug("Cache HIT for article {Id} in shard {Continent}", id, continent);
        return JsonSerializer.Deserialize<ArticleResponse>(value.ToString());
    }

    public async Task SetAsync(ArticleResponse article)
    {
        var key = BuildKey(article.Id, article.Continent);
        var json = JsonSerializer.Serialize(article);
        await _redis.StringSetAsync(key, json, Ttl);
    }

    public async Task SetBatchAsync(IEnumerable<ArticleResponse> articles)
    {
        var tasks = articles.Select(SetAsync);
        await Task.WhenAll(tasks);
    }

    public async Task RemoveAsync(string cacheKey)
    {
        await _redis.KeyDeleteAsync(cacheKey);
    }
}
