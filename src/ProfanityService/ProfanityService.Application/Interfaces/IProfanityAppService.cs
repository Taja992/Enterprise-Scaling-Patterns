using ProfanityService.Application.DTOs;

namespace ProfanityService.Application.Interfaces;

public interface IProfanityAppService
{
    Task<bool> ContainsProfanityAsync(string text);
    Task<ProfanityWordResponse> AddWordAsync(AddProfanityWordRequest request);
    Task<List<ProfanityWordResponse>> GetAllWordsAsync();
}
