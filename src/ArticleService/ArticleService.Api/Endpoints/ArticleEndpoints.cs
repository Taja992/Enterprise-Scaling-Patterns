using ArticleService.Application.DTOs.Articles;
using ArticleService.Application.Interfaces;

namespace ArticleService.Api.Endpoints;

public static class ArticleEndpoints
{
    public static RouteGroupBuilder MapArticleEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/articles").WithTags("Articles");

        api.MapGet("/", GetAllArticles)
            .WithName("Get all articles")
            .WithDescription("Fetches all articles from a specific continent.");

        api.MapGet("/{id:guid}", GetArticleById)
            .WithName("Get article by ID")
            .WithDescription("Fetches a single article by its ID and continent.");

        api.MapPost("/", CreateArticle)
            .WithName("Create article")
            .WithDescription("Creates a new article.");

        api.MapPut("/{id:guid}", UpdateArticle)
            .WithName("Update article")
            .WithDescription("Updates an existing article.");

        api.MapDelete("/{id:guid}", DeleteArticle)
            .WithName("Delete article")
            .WithDescription("Deletes an article.");

        return api;
    }

    private static async Task<IResult> GetAllArticles(
        string continent,
        IArticleAppService articleService
    )
    {
        // This could be expanded to support pagination
        return Results.Ok($"Get all articles from {continent}");
    }

    private static async Task<IResult> GetArticleById(
        Guid id,
        string continent,
        IArticleAppService articleService
    )
    {
        var result = await articleService.GetArticleAsync(id, continent);
        return result is not null
            ? Results.Ok(result)
            : Results.NotFound($"Article {id} not found in {continent}");
    }

    private static async Task<IResult> CreateArticle(
        CreateArticleRequest request,
        IArticleAppService articleService
    )
    {
        var result = await articleService.CreateArticleAsync(request);
        return Results.Created($"/api/articles/{result.Id}", result);
    }

    private static async Task<IResult> UpdateArticle(
        Guid id,
        string continent,
        UpdateArticleRequest request,
        IArticleAppService articleService
    )
    {
        var result = await articleService.UpdateArticleAsync(id, continent, request);
        return result is not null
            ? Results.Ok(result)
            : Results.NotFound($"Article {id} not found in {continent}");
    }

    private static async Task<IResult> DeleteArticle(
        Guid id,
        string continent,
        IArticleAppService articleService
    )
    {
        var deleted = await articleService.DeleteArticleAsync(id, continent);
        return deleted
            ? Results.NoContent()
            : Results.NotFound($"Article {id} not found in {continent}");
    }
}
