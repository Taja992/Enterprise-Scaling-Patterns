using ArticleService.Domain.Entities;

namespace ArticleService.Application.Interfaces;

public interface ICommentRepository
{
    Task<Comment> CreateAsync(Comment comment);
    Task<Comment?> GetByIdAsync(Guid id);
    Task<List<Comment>> GetByArticleIdAsync(Guid articleId);
}
