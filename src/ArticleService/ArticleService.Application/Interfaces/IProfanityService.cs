using ArticleService.Application.DTOs.Profanity;

namespace ArticleService.Application.Interfaces;

public interface IProfanityService
{
    Task<bool> ContainsProfanityAsync(string text);
    Task<ProfanityWordResponse> AddWordAsync(AddProfanityWordRequest request);
    Task<List<ProfanityWordResponse>> GetAllWordsAsync();
}
