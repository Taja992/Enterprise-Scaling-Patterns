using ArticleService.Application.DTOs.Articles;

namespace ArticleService.Application.Interfaces;

public interface IArticleCache
{
    Task<ArticleResponse?> GetAsync(Guid id, string continent);
    
    Task SetAsync(ArticleResponse article);
    
    Task SetBatchAsync(IEnumerable<ArticleResponse> articles);
}