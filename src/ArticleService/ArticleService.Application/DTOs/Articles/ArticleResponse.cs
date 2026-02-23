namespace ArticleService.Application.DTOs.Articles;

public record ArticleResponse(
    Guid Id,
    string Title,
    string Content,
    string Continent,
    DateTime PublishedAt,
    Guid PublisherId
);
