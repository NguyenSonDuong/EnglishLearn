using English.Entity.DTOs;

namespace English.Entity.Services;

public interface ILearningMaterialService
{
    Task<List<LearningMaterialDto>> GetByDeckIdAsync(Guid deckId);
    Task<LearningMaterialDto?> GetByIdAsync(Guid id);
    Task<LearningMaterialDto> CreateAsync(LearningMaterialDto dto);
    Task UpdateAsync(LearningMaterialDto dto);
    Task DeleteAsync(Guid id);
}
