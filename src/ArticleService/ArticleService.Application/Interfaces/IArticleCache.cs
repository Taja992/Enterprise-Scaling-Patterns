using ArticleService.Application.DTOs.Articles;

namespace ArticleService.Application.Interfaces;

public interface IArticleCache
{
    /// <summary>Returns a cached article, or null on a cache miss.</summary>
    Task<ArticleResponse?> GetAsync(Guid id, string continent);

    /// <summary>Writes a single article into the cache.</summary>
    Task SetAsync(ArticleResponse article);

    /// <summary>Writes a batch of articles — used by the background refresh job.</summary>
    Task SetBatchAsync(IEnumerable<ArticleResponse> articles);
}
