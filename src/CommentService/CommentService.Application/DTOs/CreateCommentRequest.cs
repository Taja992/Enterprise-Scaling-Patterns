namespace CommentService.Application.DTOs;

public record CreateCommentRequest(Guid ArticleId, Guid AuthorId, string Body);
