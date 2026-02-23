namespace ArticleService.Application.DTOs.Comments;

public record CommentResponse(
    Guid Id,
    Guid ArticleId,
    Guid AuthorId,
    string Body,
    DateTime CreatedAt,
    bool IsFlagged
);
