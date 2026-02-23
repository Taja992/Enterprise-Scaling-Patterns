using ArticleService.Application.DTOs.Profanity;
using ArticleService.Application.Interfaces;

namespace ArticleService.Api.Endpoints;

public static class ProfanityEndpoints
{
    public static RouteGroupBuilder MapProfanityEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/profanity").WithTags("Profanity");

        api.MapPost("/", AddProfanityWord)
            .WithName("Add profanity word")
            .WithDescription("Adds a word to the profanity filter list.");

        api.MapGet("/", GetAllProfanityWords)
            .WithName("Get all profanity words")
            .WithDescription("Returns all words on the profanity filter list.");

        return api;
    }

    private static async Task<IResult> AddProfanityWord(
        AddProfanityWordRequest request,
        IProfanityService profanityService
    )
    {
        var result = await profanityService.AddWordAsync(request);
        return Results.Created($"/api/profanity/{result.Id}", result);
    }

    private static async Task<IResult> GetAllProfanityWords(IProfanityService profanityService)
    {
        var results = await profanityService.GetAllWordsAsync();
        return Results.Ok(results);
    }
}
