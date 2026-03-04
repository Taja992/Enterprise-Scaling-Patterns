namespace CommentService.Domain.Entities;

public class Comment
{
    public Guid Id { get; private set; }
    public Guid ArticleId { get; private set; }
    public Guid AuthorId { get; private set; }
    public string Body { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public bool IsFlagged { get; private set; }

    private Comment() { }

    public static Comment Create(Guid articleId, Guid authorId, string body, bool isFlagged)
    {
        return new Comment
        {
            Id = Guid.NewGuid(),
            ArticleId = articleId,
            AuthorId = authorId,
            Body = body,
            CreatedAt = DateTime.UtcNow,
            IsFlagged = isFlagged,
        };
    }
}
