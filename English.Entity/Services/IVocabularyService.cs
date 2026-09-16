using English.Entity.DTOs;

namespace English.Entity.Services;

/// <summary>
/// Giao diện Service xử lý nghiệp vụ và các thao tác CRUD cho từ vựng (Vocabulary).
/// Tuyệt đối chỉ nhận và trả về DTOs, che giấu hoàn toàn các Entity bên trong.
/// </summary>
public interface IVocabularyService
{
    Task<VocabularyDto?> GetVocabularyByIdAsync(Guid id);
    Task<List<VocabularyDto>> GetAllVocabulariesAsync();
    Task<VocabularyDto?> CreateVocabularyAsync(VocabularyDto dto);
    Task<bool> UpdateVocabularyAsync(VocabularyDto dto);
    Task<bool> DeleteVocabularyAsync(Guid id);
}
