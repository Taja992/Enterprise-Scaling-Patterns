namespace CommentService.Application.DTOs;

public record CommentResponse(
    Guid Id,
    Guid ArticleId,
    Guid AuthorId,
    string Body,
    DateTime CreatedAt,
    bool IsFlagged
);
