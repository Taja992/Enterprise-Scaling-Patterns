using Microsoft.EntityFrameworkCore;
using ProfanityService.Application.Interfaces;
using ProfanityService.Domain.Entities;
using ProfanityService.Infrastructure.Persistence;

namespace ProfanityService.Infrastructure.Repositories;

public class ProfanityRepository : IProfanityRepository
{
    private readonly ProfanityDbContext _context;

    public ProfanityRepository(ProfanityDbContext context)
    {
        _context = context;
    }

    public async Task<ProfaneWord> AddWordAsync(ProfaneWord word)
    {
        _context.ProfaneWords.Add(word);
        await _context.SaveChangesAsync();
        return word;
    }

    public async Task<List<ProfaneWord>> GetAllWordsAsync()
    {
        return await _context.ProfaneWords.OrderBy(w => w.Word).ToListAsync();
    }

    public async Task<bool> ContainsAnyAsync(string text)
    {
        var lower = text.ToLowerInvariant();
        return await _context.ProfaneWords.AnyAsync(w => lower.Contains(w.Word));
    }
}
