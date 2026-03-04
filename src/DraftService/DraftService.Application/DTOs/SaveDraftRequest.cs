namespace DraftService.Application.DTOs;

public record SaveDraftRequest(Guid AuthorId, string Title, string Content);
