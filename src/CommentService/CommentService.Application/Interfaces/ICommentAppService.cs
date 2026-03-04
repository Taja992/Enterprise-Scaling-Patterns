using CommentService.Application.Common;
using CommentService.Application.DTOs;

namespace CommentService.Application.Interfaces;

public interface ICommentAppService
{
    Task<Result<CommentResponse>> CreateCommentAsync(CreateCommentRequest request);
    Task<Result<CommentResponse>> GetCommentAsync(Guid id);
    Task<Result<List<CommentResponse>>> GetCommentsByArticleAsync(Guid articleId);
}
