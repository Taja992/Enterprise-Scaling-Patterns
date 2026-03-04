using CommentService.Application.DTOs;
using CommentService.Application.Interfaces;

namespace CommentService.Api.Endpoints;

public static class CommentEndpoints
{
    public static RouteGroupBuilder MapCommentEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/comments").WithTags("Comments");

        api.MapPost("/", CreateComment)
            .WithName("Create comment")
            .WithDescription(
                "Creates a comment. Body is checked against ProfanityService — flagged if profane or if ProfanityService is unreachable."
            );

        api.MapGet("/{id:guid}", GetComment)
            .WithName("Get comment by ID")
            .WithDescription("Fetches a single comment by its ID.");

        // Route renamed from /api/articles/{id}/comments — CommentService owns all comment routing
        api.MapGet("/article/{articleId:guid}", GetCommentsByArticle)
            .WithName("Get comments by article")
            .WithDescription("Fetches all comments for a given article ID.");

        return api;
    }

    private static async Task<IResult> CreateComment(
        CreateCommentRequest request,
        ICommentAppService service
    )
    {
        var result = await service.CreateCommentAsync(request);
        return result.IsSuccess
            ? Results.Created($"/api/comments/{result.Value!.Id}", result.Value)
            : Results.Problem(result.Error!.Message, statusCode: 500);
    }

    private static async Task<IResult> GetComment(Guid id, ICommentAppService service)
    {
        var result = await service.GetCommentAsync(id);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.NotFound(result.Error!.Message);
    }

    private static async Task<IResult> GetCommentsByArticle(
        Guid articleId,
        ICommentAppService service
    )
    {
        var result = await service.GetCommentsByArticleAsync(articleId);
        return Results.Ok(result.Value);
    }
}
