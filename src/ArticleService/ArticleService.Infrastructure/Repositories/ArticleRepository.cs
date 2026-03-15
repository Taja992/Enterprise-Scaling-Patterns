using ArticleService.Application.Interfaces;
using ArticleService.Domain.Entities;
using ArticleService.Infrastructure.Sharding;
using Microsoft.EntityFrameworkCore;

namespace ArticleService.Infrastructure.Repositories;

public class ArticleRepository : IArticleRepository
{
    private readonly IShardResolver _shardResolver;

    public ArticleRepository(IShardResolver shardResolver)
    {
        _shardResolver = shardResolver;
    }

    public async Task<Article?> GetByIdAsync(Guid id, string continent)
    {
        var dbContext = _shardResolver.GetDbContext(continent);
        return await dbContext.Articles.FindAsync(id);
    }

    public async Task<Article> CreateAsync(Article article, string continent)
    {
        var dbContext = _shardResolver.GetDbContext(continent);
        dbContext.Articles.Add(article);
        await dbContext.SaveChangesAsync();
        return article;
    }

    public async Task UpdateAsync(Article article, string continent)
    {
        var dbContext = _shardResolver.GetDbContext(continent);
        dbContext.Articles.Update(article);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id, string continent)
    {
        var dbContext = _shardResolver.GetDbContext(continent);
        var article = await dbContext.Articles.FindAsync(id);

        if (article is not null)
        {
            dbContext.Articles.Remove(article);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task<List<Article>> GetAllByContinentAsync(string continent)
    {
        var dbContext = _shardResolver.GetDbContext(continent);
        return await dbContext.Articles.ToListAsync();
    }

    public async Task<List<Article>> GetRecentArticlesAsync(string continent, DateTime since)
    {
        var dbContext = _shardResolver.GetDbContext(continent);
        return await dbContext.Articles
            .Where(a => a.PublishedAt >= since)
            .ToListAsync();
    }
}