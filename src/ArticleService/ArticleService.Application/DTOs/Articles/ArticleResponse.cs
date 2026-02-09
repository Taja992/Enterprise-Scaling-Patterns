namespace ArticleService.Application.DTOs;

public record ArticleResponse(
    Guid Id,
    string Title,
    string Content,
    string Continent,
    DateTime PublishedAt,
    Guid PublisherId
);
