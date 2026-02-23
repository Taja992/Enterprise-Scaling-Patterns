namespace ArticleService.Application.DTOs.Articles;

public record CreateArticleRequest(
    string Title,
    string Content,
    string Continent,
    Guid PublisherId
);
