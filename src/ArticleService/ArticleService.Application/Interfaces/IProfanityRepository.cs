using ArticleService.Domain.Entities;

namespace ArticleService.Application.Interfaces;

public interface IProfanityRepository
{
    Task<ProfaneWord> AddWordAsync(ProfaneWord word);
    Task<List<ProfaneWord>> GetAllWordsAsync();
    Task<bool> ContainsAnyAsync(string text);
}
