using ProfanityService.Domain.Entities;

namespace ProfanityService.Application.Interfaces;

public interface IProfanityRepository
{
    Task<ProfaneWord> AddWordAsync(ProfaneWord word);
    Task<List<ProfaneWord>> GetAllWordsAsync();
    Task<bool> ContainsAnyAsync(string text);
}
