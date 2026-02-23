using ArticleService.Application.DTOs.Comments;
using ArticleService.Application.Interfaces;

namespace ArticleService.Api.Endpoints;

public static class CommentEndpoints
{
    public static RouteGroupBuilder MapCommentEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api").WithTags("Comments");

        api.MapPost("/comments", CreateComment)
            .WithName("Create comment")
            .WithDescription("Creates a comment and checks it for profanity.");

        api.MapGet("/comments/{id:guid}", GetCommentById)
            .WithName("Get comment by ID")
            .WithDescription("Fetches a single comment by its ID.");

        api.MapGet("/articles/{articleId:guid}/comments", GetCommentsByArticle)
            .WithName("Get comments by article")
            .WithDescription("Fetches all comments for a given article.");

        return api;
    }

    private static async Task<IResult> CreateComment(
        CreateCommentRequest request,
        ICommentService commentService
    )
    {
        var result = await commentService.CreateCommentAsync(request);
        return Results.Created($"/api/comments/{result.Id}", result);
    }

    private static async Task<IResult> GetCommentById(Guid id, ICommentService commentService)
    {
        var result = await commentService.GetCommentAsync(id);
        return result is not null
            ? Results.Ok(result)
            : Results.NotFound($"Comment {id} not found");
    }

    private static async Task<IResult> GetCommentsByArticle(
        Guid articleId,
        ICommentService commentService
    )
    {
        var results = await commentService.GetCommentsByArticleAsync(articleId);
        return Results.Ok(results);
    }
}
