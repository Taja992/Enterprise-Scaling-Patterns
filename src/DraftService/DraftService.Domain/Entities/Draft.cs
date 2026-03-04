namespace DraftService.Domain.Entities;

public class Draft
{
    public Guid Id { get; private set; }
    public Guid AuthorId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Draft() { }

    public static Draft Create(Guid authorId, string title, string content)
    {
        var now = DateTime.UtcNow;
        return new Draft
        {
            Id = Guid.NewGuid(),
            AuthorId = authorId,
            Title = title,
            Content = content,
            CreatedAt = now,
            UpdatedAt = now,
        };
    }

    public void Update(string title, string content)
    {
        Title = title;
        Content = content;
        UpdatedAt = DateTime.UtcNow;
    }
}
