namespace DraftService.Application.DTOs;

public record DraftResponse(
    Guid Id,
    Guid AuthorId,
    string Title,
    string Content,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
