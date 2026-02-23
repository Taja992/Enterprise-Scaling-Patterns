using ArticleService.Application.Common;
using ArticleService.Application.DTOs.Comments;
using ArticleService.Application.Interfaces;
using ArticleService.Domain.Entities;

namespace ArticleService.Application.Services;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly IProfanityService _profanityService;
    private readonly CommentCircuitBreaker _circuitBreaker;

    public CommentService(
        ICommentRepository commentRepository,
        IProfanityService profanityService,
        CommentCircuitBreaker circuitBreaker
    )
    {
        _commentRepository = commentRepository;
        _profanityService = profanityService;
        _circuitBreaker = circuitBreaker;
    }

    public async Task<CommentResponse> CreateCommentAsync(CreateCommentRequest request)
    {
        // Circuit breaker returns null when open — flag the comment as fallback
        var hasProfanity = await _circuitBreaker.ExecuteAsync(() =>
            _profanityService.ContainsProfanityAsync(request.Body)
        );

        var isFlagged = hasProfanity ?? true;

        var comment = Comment.Create(request.ArticleId, request.AuthorId, request.Body, isFlagged);
        var created = await _commentRepository.CreateAsync(comment);
        return MapToResponse(created);
    }

    public async Task<CommentResponse?> GetCommentAsync(Guid id)
    {
        var comment = await _commentRepository.GetByIdAsync(id);
        return comment is not null ? MapToResponse(comment) : null;
    }

    public async Task<List<CommentResponse>> GetCommentsByArticleAsync(Guid articleId)
    {
        var comments = await _commentRepository.GetByArticleIdAsync(articleId);
        return comments.Select(MapToResponse).ToList();
    }

    private static CommentResponse MapToResponse(Comment comment)
    {
        return new CommentResponse(
            comment.Id,
            comment.ArticleId,
            comment.AuthorId,
            comment.Body,
            comment.CreatedAt,
            comment.IsFlagged
        );
    }
}
