using DraftService.Application.Interfaces;
using DraftService.Domain.Entities;
using DraftService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DraftService.Infrastructure.Repositories;

public class DraftRepository : IDraftRepository
{
    private readonly DraftDbContext _dbContext;

    public DraftRepository(DraftDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Draft> SaveAsync(Draft draft)
    {
        _dbContext.Drafts.Add(draft);
        await _dbContext.SaveChangesAsync();
        return draft;
    }

    public async Task<Draft?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Drafts.FindAsync(id);
    }

    public async Task<List<Draft>> GetByAuthorIdAsync(Guid authorId)
    {
        return await _dbContext
            .Drafts.Where(d => d.AuthorId == authorId)
            .OrderByDescending(d => d.UpdatedAt)
            .ToListAsync();
    }

    public async Task<Draft?> UpdateAsync(Draft draft)
    {
        _dbContext.Drafts.Update(draft);
        await _dbContext.SaveChangesAsync();
        return draft;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var draft = await _dbContext.Drafts.FindAsync(id);
        if (draft is null)
            return false;

        _dbContext.Drafts.Remove(draft);
        await _dbContext.SaveChangesAsync();
        return true;
    }
}
