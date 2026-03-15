using CommentService.Application.DTOs;

namespace CommentService.Application.Interfaces;

public interface ICommentCache
{
    Task<List<CommentResponse>?> GetByArticleAsync(Guid articleId);

    Task SetByArticleAsync(Guid articleId, List<CommentResponse> comments);
}
