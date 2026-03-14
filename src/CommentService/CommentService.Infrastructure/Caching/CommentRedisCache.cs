using System.Text.Json;
using CommentService.Application.DTOs;
using CommentService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Prometheus;
using StackExchange.Redis;

namespace CommentService.Infrastructure.Caching;

/// <summary>
/// Redis-backed CommentCache with Least Recently Used (LRU) eviction.
///
/// Data layout in Redis:
///   comment:article:{articleId}  →  JSON string (list of CommentResponse)
///   comment:lru                  →  Sorted Set — member=articleId, score=UnixTimestamp
///
/// Eviction: when the sorted set reaches capacity (30), the member with the
/// lowest score (least recently used) is removed along with its data key.
/// </summary>
public class CommentRedisCache : ICommentCache
{
    // ── Prometheus metrics ────────────────────────────────────────────────────
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

    // ── Constants ─────────────────────────────────────────────────────────────
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

    private static double NowScore() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    // ─────────────────────────────────────────────────────────────────────────
    // GET — return cached comments and refresh the LRU timestamp
    // ─────────────────────────────────────────────────────────────────────────
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

        // Refresh the LRU score so this article is not evicted soon.
        await _redis.SortedSetAddAsync(LruKey, articleId.ToString(), NowScore());

        CacheHits.Inc();
        _logger.LogDebug("Comment cache HIT for article {ArticleId}", articleId);
        return JsonSerializer.Deserialize<List<CommentResponse>>(value.ToString());
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SET — store comments, evict LRU if at capacity
    // ─────────────────────────────────────────────────────────────────────────
    public async Task SetByArticleAsync(Guid articleId, List<CommentResponse> comments)
    {
        // ── Eviction check ────────────────────────────────────────────────────
        var currentSize = await _redis.SortedSetLengthAsync(LruKey);

        if (currentSize >= Capacity)
        {
            // ZRANGE with RangeByRank returns members sorted by score ascending.
            // Index 0 is the member with the smallest (oldest) score = least recently used.
            var lruEntries = await _redis.SortedSetRangeByRankAsync(LruKey, 0, 0);

            if (lruEntries.Length > 0)
            {
                var evictedId = lruEntries[0].ToString();

                // Remove data key and LRU entry atomically-ish.
                // (For true atomicity, a Lua script would be needed — acceptable trade-off here.)
                await _redis.KeyDeleteAsync(DataKey(Guid.Parse(evictedId)));
                await _redis.SortedSetRemoveAsync(LruKey, evictedId);

                _logger.LogDebug(
                    "CommentCache full — evicted LRU article {EvictedArticleId}",
                    evictedId
                );
            }
        }

        // ── Write data ────────────────────────────────────────────────────────
        var json = JsonSerializer.Serialize(comments);
        await _redis.StringSetAsync(DataKey(articleId), json);

        // ── Update LRU sorted set ─────────────────────────────────────────────
        // ZADD will add if not present, or update the score if already present.
        await _redis.SortedSetAddAsync(LruKey, articleId.ToString(), NowScore());

        // ── Update gauge ──────────────────────────────────────────────────────
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
