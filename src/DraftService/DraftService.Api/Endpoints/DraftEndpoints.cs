using DraftService.Application.DTOs;
using DraftService.Application.Services;

namespace DraftService.Api.Endpoints;

public static class DraftEndpoints
{
    public static RouteGroupBuilder MapDraftEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/drafts").WithTags("Drafts");

        api.MapPost("/", SaveDraft)
            .WithName("Save draft")
            .WithDescription("Creates a new draft for an author.");

        api.MapGet("/{id:guid}", GetDraft)
            .WithName("Get draft by ID")
            .WithDescription("Fetches a single draft by its ID.");

        api.MapGet("/author/{authorId:guid}", GetDraftsByAuthor)
            .WithName("Get drafts by author")
            .WithDescription("Fetches all drafts belonging to a given author.");

        api.MapPut("/{id:guid}", UpdateDraft)
            .WithName("Update draft")
            .WithDescription("Updates the title and content of an existing draft.");

        api.MapDelete("/{id:guid}", DeleteDraft)
            .WithName("Delete draft")
            .WithDescription("Permanently deletes a draft.");

        return api;
    }

    private static async Task<IResult> SaveDraft(
        SaveDraftRequest request,
        DraftAppService service
    )
    {
        var result = await service.SaveDraftAsync(request);
        return result.IsSuccess
            ? Results.Created($"/api/drafts/{result.Value!.Id}", result.Value)
            : Results.Problem(result.Error!.Message, statusCode: 500);
    }

    private static async Task<IResult> GetDraft(Guid id, DraftAppService service)
    {
        var result = await service.GetDraftAsync(id);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.NotFound(result.Error!.Message);
    }

    private static async Task<IResult> GetDraftsByAuthor(Guid authorId, DraftAppService service)
    {
        var result = await service.GetDraftsByAuthorAsync(authorId);
        return Results.Ok(result.Value);
    }

    private static async Task<IResult> UpdateDraft(
        Guid id,
        UpdateDraftRequest request,
        DraftAppService service
    )
    {
        var result = await service.UpdateDraftAsync(id, request);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.NotFound(result.Error!.Message);
    }

    private static async Task<IResult> DeleteDraft(Guid id, DraftAppService service)
    {
        var result = await service.DeleteDraftAsync(id);
        return result.IsSuccess ? Results.NoContent() : Results.NotFound(result.Error!.Message);
    }
}