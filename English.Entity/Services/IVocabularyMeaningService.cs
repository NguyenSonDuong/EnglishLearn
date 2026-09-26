using English.Entity.DTOs;

namespace English.Entity.Services;

/// <summary>
/// Giao diện Service xử lý nghiệp vụ và các thao tác CRUD cho ngữ nghĩa từ vựng (VocabularyMeaning).
/// Che giấu hoàn toàn các Entity bên trong, chỉ trao đổi qua DTO.
/// </summary>
public interface IVocabularyMeaningService
{
    Task<VocabularyMeaningDto?> GetMeaningByIdAsync(Guid id);
    Task<List<VocabularyMeaningDto>> GetAllMeaningsAsync();
    Task<List<VocabularyMeaningDto>> GetMeaningsByVocabularyIdAsync(Guid vocabularyId);
    Task<VocabularyMeaningDto?> CreateMeaningAsync(VocabularyMeaningDto dto);
    Task<bool> UpdateMeaningAsync(VocabularyMeaningDto dto);
    Task<bool> DeleteMeaningAsync(Guid id);
}
