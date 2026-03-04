using ProfanityService.Application.DTOs;
using ProfanityService.Application.Interfaces;

namespace ProfanityService.Api.Endpoints;

public static class ProfanityEndpoints
{
    public static RouteGroupBuilder MapProfanityEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/profanity").WithTags("Profanity");

        api.MapPost("/", AddWord)
            .WithName("Add profanity word")
            .WithDescription("Adds a word to the profanity list. Stored as lowercase.");

        api.MapGet("/", GetAllWords)
            .WithName("Get all profanity words")
            .WithDescription("Returns all words currently in the profanity list.");

        // Internal endpoint — called by CommentService via HTTP to check a comment body
        api.MapGet("/check", CheckText)
            .WithName("Check text for profanity")
            .WithDescription("Returns whether the provided text contains any profane words.");

        return api;
    }

    private static async Task<IResult> AddWord(
        AddProfanityWordRequest request,
        IProfanityAppService service
    )
    {
        var result = await service.AddWordAsync(request);
        return Results.Created($"/api/profanity/{result.Id}", result);
    }

    private static async Task<IResult> GetAllWords(IProfanityAppService service)
    {
        var words = await service.GetAllWordsAsync();
        return Results.Ok(words);
    }

    private static async Task<IResult> CheckText(string text, IProfanityAppService service)
    {
        var hasProfanity = await service.ContainsProfanityAsync(text);
        return Results.Ok(new ProfanityCheckResponse(hasProfanity));
    }
}
