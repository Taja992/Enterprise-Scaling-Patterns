using DraftService.Domain.Entities;

namespace DraftService.Application.Interfaces;

public interface IDraftRepository
{
    Task<Draft> SaveAsync(Draft draft);
    Task<Draft?> GetByIdAsync(Guid id);
    Task<List<Draft>> GetByAuthorIdAsync(Guid authorId);
    Task<Draft?> UpdateAsync(Draft draft);
    Task<bool> DeleteAsync(Guid id);
}
