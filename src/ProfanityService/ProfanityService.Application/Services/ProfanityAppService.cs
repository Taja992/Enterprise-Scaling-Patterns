using Microsoft.Extensions.Logging;
using ProfanityService.Application.DTOs;
using ProfanityService.Application.Interfaces;
using ProfanityService.Domain.Entities;

namespace ProfanityService.Application.Services;

public class ProfanityAppService : IProfanityAppService
{
    private readonly IProfanityRepository _repository;
    private readonly ILogger<ProfanityAppService> _logger;

    public ProfanityAppService(IProfanityRepository repository, ILogger<ProfanityAppService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<bool> ContainsProfanityAsync(string text)
    {
        _logger.LogInformation("Checking text for profanity (length {Length})", text.Length);
        var result = await _repository.ContainsAnyAsync(text);
        _logger.LogInformation("Profanity check complete — hasProfanity: {HasProfanity}", result);
        return result;
    }

    public async Task<ProfanityWordResponse> AddWordAsync(AddProfanityWordRequest request)
    {
        _logger.LogInformation("Adding profanity word");
        var word = ProfaneWord.Create(request.Word);
        var saved = await _repository.AddWordAsync(word);
        _logger.LogInformation("Profanity word {WordId} added", saved.Id);
        return new ProfanityWordResponse(saved.Id, saved.Word);
    }

    public async Task<List<ProfanityWordResponse>> GetAllWordsAsync()
    {
        _logger.LogInformation("Fetching all profanity words");
        var words = await _repository.GetAllWordsAsync();
        _logger.LogInformation("Fetched {Count} profanity word(s)", words.Count);
        return words.Select(w => new ProfanityWordResponse(w.Id, w.Word)).ToList();
    }
}
