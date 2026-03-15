using CommentService.Application.Common;
using CommentService.Application.DTOs;
using CommentService.Application.Interfaces;
using CommentService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace CommentService.Application.Services;

public class CommentAppService : ICommentAppService
{
    private readonly ICommentRepository _repository;
    private readonly IProfanityServiceClient _profanityClient;
    private readonly CommentCircuitBreaker _circuitBreaker;
    private readonly ICommentCache _cache;
    private readonly ILogger<CommentAppService> _logger;

    public CommentAppService(
        ICommentRepository repository,
        IProfanityServiceClient profanityClient,
        CommentCircuitBreaker circuitBreaker,
        ICommentCache cache,
        ILogger<CommentAppService> logger
    )
    {
        _repository = repository;
        _profanityClient = profanityClient;
        _circuitBreaker = circuitBreaker;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Result<CommentResponse>> CreateCommentAsync(CreateCommentRequest request)
    {
        _logger.LogInformation(
            "Creating comment for article {ArticleId} by author {AuthorId}",
            request.ArticleId,
            request.AuthorId
        );

        try
        {
            // Circuit breaker wraps the HTTP call to ProfanityService.
            // Returns null if the circuit is open — treat as flagged (fail safe).
            var hasProfanity = await _circuitBreaker.ExecuteAsync(() =>
                _profanityClient.ContainsProfanityAsync(request.Body)
            );

            if (hasProfanity is null)
                _logger.LogWarning(
                    "ProfanityService circuit is open — comment will be flagged by default"
                );

            var isFlagged = hasProfanity ?? true;

            var comment = Comment.Create(
                request.ArticleId,
                request.AuthorId,
                request.Body,
                isFlagged
            );
            var saved = await _repository.CreateAsync(comment);

            _logger.LogInformation(
                "Comment {CommentId} created — IsFlagged: {IsFlagged}",
                saved.Id,
                saved.IsFlagged
            );

            return Result<CommentResponse>.Success(MapToResponse(saved));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to create comment for article {ArticleId}",
                request.ArticleId
            );
            return Result<CommentResponse>.Failure(
                AppError.InternalError("An error occurred while creating the comment.")
            );
        }
    }

    public async Task<Result<CommentResponse>> GetCommentAsync(Guid id)
    {
        _logger.LogInformation("Fetching comment {CommentId}", id);

        var comment = await _repository.GetByIdAsync(id);

        if (comment is null)
        {
            _logger.LogWarning("Comment {CommentId} not found", id);
            return Result<CommentResponse>.Failure(AppError.NotFound($"Comment {id} not found."));
        }

        return Result<CommentResponse>.Success(MapToResponse(comment));
    }

    public async Task<Result<List<CommentResponse>>> GetCommentsByArticleAsync(Guid articleId)
    {
        _logger.LogInformation("Fetching comments for article {ArticleId}", articleId);

        var cached = await _cache.GetByArticleAsync(articleId);
        if (cached is not null)
        {
            _logger.LogDebug(
                "Comments for article {ArticleId} served from cache ({Count} comments)",
                articleId,
                cached.Count
            );
            return Result<List<CommentResponse>>.Success(cached);
        }

        _logger.LogDebug(
            "Comments for article {ArticleId} not in cache — querying database",
            articleId
        );

        var comments = await _repository.GetByArticleIdAsync(articleId);

        _logger.LogInformation(
            "Fetched {Count} comment(s) for article {ArticleId}",
            comments.Count,
            articleId
        );

        var responses = comments.Select(MapToResponse).ToList();

        await _cache.SetByArticleAsync(articleId, responses);

        return Result<List<CommentResponse>>.Success(responses);
    }

    private static CommentResponse MapToResponse(Comment c) =>
        new(c.Id, c.ArticleId, c.AuthorId, c.Body, c.CreatedAt, c.IsFlagged);
}