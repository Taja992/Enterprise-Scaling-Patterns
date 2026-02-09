namespace ArticleService.Domain.Entities;

public class Article
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public string Continent { get; private set; } = string.Empty;
    public DateTime PublishedAt { get; private set; }
    public Guid PublisherId { get; private set; }

    // Private constructor for EF Core
    private Article() { }

    public static Article Create(string title, string content, string continent, Guid publisherId)
    {
        return new Article
        {
            Id = Guid.NewGuid(),
            Title = title,
            Content = content,
            Continent = continent,
            PublisherId = publisherId,
            PublishedAt = DateTime.UtcNow,
        };
    }

    public void Update(string title, string content)
    {
        Title = title;
        Content = content;
    }
}
