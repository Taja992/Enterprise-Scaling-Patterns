using System.Net.Http.Json;
using CommentService.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace CommentService.Infrastructure.Http;

public class ProfanityServiceClient : IProfanityServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProfanityServiceClient> _logger;

    public ProfanityServiceClient(HttpClient httpClient, ILogger<ProfanityServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<bool> ContainsProfanityAsync(string text)
    {
        var encoded = Uri.EscapeDataString(text);
        _logger.LogInformation("Calling ProfanityService check endpoint");

        var response = await _httpClient.GetFromJsonAsync<ProfanityCheckResult>(
            $"/api/profanity/check?text={encoded}"
        );

        return response?.HasProfanity ?? false;
    }

    // Internal DTO matching the JSON shape returned by ProfanityService
    private record ProfanityCheckResult(bool HasProfanity);
}
