using ArticleService.Application.Interfaces;
using ArticleService.Domain.Entities;
using ArticleService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ArticleService.Infrastructure.Repositories;

public class ProfanityRepository : IProfanityRepository
{
    private readonly ProfanityDbContext _dbContext;

    public ProfanityRepository(ProfanityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProfaneWord> AddWordAsync(ProfaneWord word)
    {
        _dbContext.ProfaneWords.Add(word);
        await _dbContext.SaveChangesAsync();
        return word;
    }

    public async Task<List<ProfaneWord>> GetAllWordsAsync()
    {
        return await _dbContext.ProfaneWords.ToListAsync();
    }

    public async Task<bool> ContainsAnyAsync(string text)
    {
        var lower = text.ToLowerInvariant();
        return await _dbContext.ProfaneWords.AnyAsync(w => lower.Contains(w.Word));
    }
}
