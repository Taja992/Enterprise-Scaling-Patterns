using ArticleService.Application.Interfaces;
using ArticleService.Domain.Entities;
using ArticleService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ArticleService.Infrastructure.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly CommentDbContext _dbContext;

    public CommentRepository(CommentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Comment> CreateAsync(Comment comment)
    {
        _dbContext.Comments.Add(comment);
        await _dbContext.SaveChangesAsync();
        return comment;
    }

    public async Task<Comment?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Comments.FindAsync(id);
    }

    public async Task<List<Comment>> GetByArticleIdAsync(Guid articleId)
    {
        return await _dbContext.Comments.Where(c => c.ArticleId == articleId).ToListAsync();
    }
}
