namespace ArticleService.Application.DTOs.Comments;

public record CreateCommentRequest(Guid ArticleId, Guid AuthorId, string Body);
