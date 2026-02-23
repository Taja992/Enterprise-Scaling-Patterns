using ArticleService.Application.DTOs.Profanity;
using ArticleService.Application.Interfaces;
using ArticleService.Domain.Entities;

namespace ArticleService.Application.Services;

public class ProfanityService : IProfanityService
{
    private readonly IProfanityRepository _profanityRepository;

    public ProfanityService(IProfanityRepository profanityRepository)
    {
        _profanityRepository = profanityRepository;
    }

    public async Task<bool> ContainsProfanityAsync(string text)
    {
        return await _profanityRepository.ContainsAnyAsync(text);
    }

    public async Task<ProfanityWordResponse> AddWordAsync(AddProfanityWordRequest request)
    {
        var word = ProfaneWord.Create(request.Word);
        var created = await _profanityRepository.AddWordAsync(word);
        return new ProfanityWordResponse(created.Id, created.Word);
    }

    public async Task<List<ProfanityWordResponse>> GetAllWordsAsync()
    {
        var words = await _profanityRepository.GetAllWordsAsync();
        return words.Select(w => new ProfanityWordResponse(w.Id, w.Word)).ToList();
    }
}
