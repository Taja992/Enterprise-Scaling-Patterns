using DraftService.Application.Common;
using DraftService.Application.DTOs;
using DraftService.Application.Interfaces;
using DraftService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace DraftService.Application.Services;

public class DraftAppService
{
    private readonly IDraftRepository _repository;
    private readonly ILogger<DraftAppService> _logger;

    public DraftAppService(IDraftRepository repository, ILogger<DraftAppService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<DraftResponse>> SaveDraftAsync(SaveDraftRequest request)
    {
        // Information: a meaningful business event is beginning
        _logger.LogInformation(
            "Saving new draft for author {AuthorId} with title {Title}",
            request.AuthorId,
            request.Title
        );

        try
        {
            var draft = Draft.Create(request.AuthorId, request.Title, request.Content);
            var saved = await _repository.SaveAsync(draft);

            _logger.LogInformation(
                "Draft {DraftId} saved successfully for author {AuthorId}",
                saved.Id,
                saved.AuthorId
            );

            return Result<DraftResponse>.Success(MapToResponse(saved));
        }
        catch (Exception ex)
        {
            // Error: unexpected failure — include the exception for full stack in Seq
            _logger.LogError(ex, "Failed to save draft for author {AuthorId}", request.AuthorId);
            return Result<DraftResponse>.Failure(
                AppError.InternalError("An error occurred while saving the draft.")
            );
        }
    }

    public async Task<Result<DraftResponse>> GetDraftAsync(Guid id)
    {
        _logger.LogInformation("Fetching draft {DraftId}", id);

        var draft = await _repository.GetByIdAsync(id);

        if (draft is null)
        {
            // Warning: a business rule was violated (not found) — not an application error
            _logger.LogWarning("Draft {DraftId} was not found", id);
            return Result<DraftResponse>.Failure(AppError.NotFound($"Draft {id} not found."));
        }

        return Result<DraftResponse>.Success(MapToResponse(draft));
    }

    public async Task<Result<List<DraftResponse>>> GetDraftsByAuthorAsync(Guid authorId)
    {
        _logger.LogInformation("Fetching all drafts for author {AuthorId}", authorId);

        var drafts = await _repository.GetByAuthorIdAsync(authorId);

        _logger.LogInformation(
            "Fetched {DraftCount} draft(s) for author {AuthorId}",
            drafts.Count,
            authorId
        );

        return Result<List<DraftResponse>>.Success(drafts.Select(MapToResponse).ToList());
    }

    public async Task<Result<DraftResponse>> UpdateDraftAsync(Guid id, UpdateDraftRequest request)
    {
        _logger.LogInformation("Updating draft {DraftId}", id);

        var existing = await _repository.GetByIdAsync(id);

        if (existing is null)
        {
            _logger.LogWarning("Draft {DraftId} not found for update", id);
            return Result<DraftResponse>.Failure(AppError.NotFound($"Draft {id} not found."));
        }

        existing.Update(request.Title, request.Content);
        var updated = await _repository.UpdateAsync(existing);

        _logger.LogInformation("Draft {DraftId} updated successfully", id);

        return Result<DraftResponse>.Success(MapToResponse(updated!));
    }

    public async Task<Result<bool>> DeleteDraftAsync(Guid id)
    {
        _logger.LogInformation("Deleting draft {DraftId}", id);

        var deleted = await _repository.DeleteAsync(id);

        if (!deleted)
        {
            _logger.LogWarning("Draft {DraftId} not found for deletion", id);
            return Result<bool>.Failure(AppError.NotFound($"Draft {id} not found."));
        }

        _logger.LogInformation("Draft {DraftId} deleted successfully", id);
        return Result<bool>.Success(true);
    }

    private static DraftResponse MapToResponse(Draft draft) =>
        new(draft.Id, draft.AuthorId, draft.Title, draft.Content, draft.CreatedAt, draft.UpdatedAt);
}
