using CommentService.Application.DTOs;

namespace CommentService.Application.Interfaces;

public interface ICommentCache
{
    /// <summary>
    /// Returns cached comments for the given article, or null on a cache miss.
    /// Also updates the LRU access timestamp.
    /// </summary>
    Task<List<CommentResponse>?> GetByArticleAsync(Guid articleId);

    /// <summary>
    /// Stores comments for the given article.
    /// Evicts the least-recently-used article if the cache is at capacity.
    /// </summary>
    Task SetByArticleAsync(Guid articleId, List<CommentResponse> comments);
}
