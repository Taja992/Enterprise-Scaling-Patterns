namespace ArticleService.Application.DTOs;

public record CreateArticleRequest(
    string Title,
    string Content,
    string Continent,
    Guid PublisherId
);
