using DraftService.Application.Common;
using DraftService.Application.DTOs;

namespace DraftService.Application.Interfaces;

public interface IDraftAppService
{
    Task<Result<DraftResponse>> SaveDraftAsync(SaveDraftRequest request);
    Task<Result<DraftResponse>> GetDraftAsync(Guid id);
    Task<Result<List<DraftResponse>>> GetDraftsByAuthorAsync(Guid authorId);
    Task<Result<DraftResponse>> UpdateDraftAsync(Guid id, UpdateDraftRequest request);
    Task<Result<bool>> DeleteDraftAsync(Guid id);
}
