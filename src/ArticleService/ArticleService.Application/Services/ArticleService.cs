using ArticleService.Application.DTOs.Articles;
using ArticleService.Application.Interfaces;
using ArticleService.Domain.Entities;

namespace ArticleService.Application.Services;

public class ArticleAppService : IArticleAppService
{
    private readonly IArticleRepository _repository;

    public ArticleAppService(IArticleRepository repository)
    {
        _repository = repository;
    }

    public async Task<ArticleResponse> CreateArticleAsync(CreateArticleRequest request)
    {
        var article = Article.Create(
            request.Title,
            request.Content,
            request.Continent,
            request.PublisherId
        );

        var created = await _repository.CreateAsync(article, request.Continent);

        return MapToResponse(created);
    }

    public async Task<ArticleResponse?> GetArticleAsync(Guid id, string continent)
    {
        var article = await _repository.GetByIdAsync(id, continent);
        return article is not null ? MapToResponse(article) : null;
    }

    public async Task<ArticleResponse?> UpdateArticleAsync(
        Guid id,
        string continent,
        UpdateArticleRequest request
    )
    {
        var article = await _repository.GetByIdAsync(id, continent);

        if (article is null)
            return null;

        article.Update(request.Title, request.Content);
        await _repository.UpdateAsync(article, continent);

        return MapToResponse(article);
    }

    public async Task<bool> DeleteArticleAsync(Guid id, string continent)
    {
        var article = await _repository.GetByIdAsync(id, continent);

        if (article is null)
            return false;

        await _repository.DeleteAsync(id, continent);
        return true;
    }

    private static ArticleResponse MapToResponse(Article article)
    {
        return new ArticleResponse(
            article.Id,
            article.Title,
            article.Content,
            article.Continent,
            article.PublishedAt,
            article.PublisherId
        );
    }
}
