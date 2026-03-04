using CommentService.Application.Interfaces;
using CommentService.Domain.Entities;
using CommentService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CommentService.Infrastructure.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly CommentDbContext _context;

    public CommentRepository(CommentDbContext context)
    {
        _context = context;
    }

    public async Task<Comment> CreateAsync(Comment comment)
    {
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
        return comment;
    }

    public async Task<Comment?> GetByIdAsync(Guid id)
    {
        return await _context.Comments.FindAsync(id);
    }

    public async Task<List<Comment>> GetByArticleIdAsync(Guid articleId)
    {
        return await _context
            .Comments.Where(c => c.ArticleId == articleId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }
}
