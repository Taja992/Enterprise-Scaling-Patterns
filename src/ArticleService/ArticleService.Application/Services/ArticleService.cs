using ArticleService.Application.DTOs.Articles;
using ArticleService.Application.Interfaces;
using ArticleService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ArticleService.Application.Services;

public class ArticleAppService : IArticleAppService
{
    private readonly IArticleRepository _repository;
    private readonly IArticleCache _cache;
    private readonly ILogger<ArticleAppService> _logger;

    public ArticleAppService(
        IArticleRepository repository,
        IArticleCache cache,
        ILogger<ArticleAppService> logger
    )
    {
        _repository = repository;
        _cache = cache;
        _logger = logger;
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

        await _cache.SetAsync(MapToResponse(created));

        return MapToResponse(created);
    }

    public async Task<ArticleResponse?> GetArticleAsync(Guid id, string continent)
    {
        var cached = await _cache.GetAsync(id, continent);

        if (cached is not null)
        {
            _logger.LogDebug(
                "Article {Id} served from cache (shard: {Continent})",
                id,
                continent
            );

            return cached;
        }

        _logger.LogDebug(
            "Article {Id} not in cache — querying database (shard: {Continent})",
            id,
            continent
        );

        var article = await _repository.GetByIdAsync(id, continent);

        if (article is null)
            return null;

        var response = MapToResponse(article);

        await _cache.SetAsync(response);

        return response;
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

        var cacheKey = $"article:{continent}:{id}";

        await _cache.RemoveAsync(cacheKey);

        _logger.LogInformation(
            "Cache invalidated for article {Id} in shard {Continent}",
            id,
            continent
        );

        return MapToResponse(article);
    }

    public async Task<bool> DeleteArticleAsync(Guid id, string continent)
    {
        var article = await _repository.GetByIdAsync(id, continent);

        if (article is null)
            return false;

        await _repository.DeleteAsync(id, continent);

        var cacheKey = $"article:{continent}:{id}";
        await _cache.RemoveAsync(cacheKey);

        _logger.LogInformation(
            "Cache invalidated after delete for article {Id} in shard {Continent}",
            id,
            continent
        );

        return true;
    }

    public async Task<List<ArticleResponse>> GetAllArticlesAsync(string continent)
    {
        _logger.LogInformation(
            "Fetching all articles for shard {Continent}",
            continent
        );

        var articles = await _repository.GetAllByContinentAsync(continent);

        return articles.Select(MapToResponse).ToList();
    }

    private static ArticleResponse MapToResponse(Article article) =>
        new(
            article.Id,
            article.Title,
            article.Content,
            article.Continent,
            article.PublishedAt,
            article.PublisherId
        );
}