using System.Text.Json;
using CommentService.Application.DTOs;
using CommentService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Prometheus;
using StackExchange.Redis;

namespace CommentService.Infrastructure.Caching;

public class CommentRedisCache : ICommentCache
{
    private static readonly Counter CacheHits = Metrics.CreateCounter(
        "comment_cache_hits_total",
        "Total number of CommentCache hits."
    );

    private static readonly Counter CacheMisses = Metrics.CreateCounter(
        "comment_cache_misses_total",
        "Total number of CommentCache misses."
    );

    private static readonly Gauge CacheSize = Metrics.CreateGauge(
        "comment_cache_size_articles",
        "Number of articles currently held in the CommentCache."
    );

    private const int Capacity = 30;
    private const string LruKey = "comment:lru";

    private readonly IDatabase _redis;
    private readonly ILogger<CommentRedisCache> _logger;

    public CommentRedisCache(IConnectionMultiplexer connection, ILogger<CommentRedisCache> logger)
    {
        _redis = connection.GetDatabase();
        _logger = logger;
    }

    private static string DataKey(Guid articleId) => $"comment:article:{articleId}";

    private static double NowScore() =>
        DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    public async Task<List<CommentResponse>?> GetByArticleAsync(Guid articleId)
    {
        var key = DataKey(articleId);
        var value = await _redis.StringGetAsync(key);

        if (value.IsNullOrEmpty)
        {
            CacheMisses.Inc();
            _logger.LogDebug("Comment cache MISS for article {ArticleId}", articleId);
            return null;
        }

        await _redis.SortedSetAddAsync(LruKey, articleId.ToString(), NowScore());

        CacheHits.Inc();
        _logger.LogDebug("Comment cache HIT for article {ArticleId}", articleId);
        return JsonSerializer.Deserialize<List<CommentResponse>>(value.ToString());
    }

    public async Task SetByArticleAsync(Guid articleId, List<CommentResponse> comments)
    {
        var currentSize = await _redis.SortedSetLengthAsync(LruKey);

        if (currentSize >= Capacity)
        {
            var lruEntries = await _redis.SortedSetRangeByRankAsync(LruKey, 0, 0);

            if (lruEntries.Length > 0)
            {
                var evictedId = lruEntries[0].ToString();

                
                await _redis.KeyDeleteAsync(DataKey(Guid.Parse(evictedId)));
                await _redis.SortedSetRemoveAsync(LruKey, evictedId);

                _logger.LogDebug(
                    "CommentCache full — evicted LRU article {EvictedArticleId}",
                    evictedId
                );
            }
        }

        var json = JsonSerializer.Serialize(comments);
        await _redis.StringSetAsync(DataKey(articleId), json);

        await _redis.SortedSetAddAsync(LruKey, articleId.ToString(), NowScore());

        var newSize = await _redis.SortedSetLengthAsync(LruKey);
        CacheSize.Set(newSize);

        _logger.LogDebug(
            "Stored comments for article {ArticleId} — cache holds {Size}/{Capacity} articles",
            articleId,
            newSize,
            Capacity
        );
    }
}