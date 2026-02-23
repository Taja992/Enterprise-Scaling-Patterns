using ArticleService.Application.DTOs.Comments;

namespace ArticleService.Application.Interfaces;

public interface ICommentService
{
    Task<CommentResponse> CreateCommentAsync(CreateCommentRequest request);
    Task<CommentResponse?> GetCommentAsync(Guid id);
    Task<List<CommentResponse>> GetCommentsByArticleAsync(Guid articleId);
}
