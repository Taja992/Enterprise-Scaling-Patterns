using ArticleService.Domain.Entities;

namespace ArticleService.Application.Interfaces;

public interface IArticleRepository
{
    Task<Article?> GetByIdAsync(Guid id, string continent);
    Task<Article> CreateAsync(Article article, string continent);
    Task UpdateAsync(Article article, string continent);
    Task DeleteAsync(Guid id, string continent);
    Task<List<Article>> GetAllByContinentAsync(string continent);
    Task<List<Article>> GetRecentArticlesAsync(string continent, DateTime since); 
}
