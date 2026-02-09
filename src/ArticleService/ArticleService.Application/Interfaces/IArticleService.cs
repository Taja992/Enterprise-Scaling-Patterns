using ArticleService.Application.DTOs;

namespace ArticleService.Application.Interfaces;

public interface IArticleService
{
    Task<ArticleResponse> CreateArticleAsync(CreateArticleRequest request);
    Task<ArticleResponse?> GetArticleAsync(Guid id, string continent);
    Task<ArticleResponse?> UpdateArticleAsync(
        Guid id,
        string continent,
        UpdateArticleRequest request
    );
    Task<bool> DeleteArticleAsync(Guid id, string continent);
}
