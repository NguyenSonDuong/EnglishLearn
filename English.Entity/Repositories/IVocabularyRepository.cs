using English.Entity.Entities;
using English.Entity.Enums;

namespace English.Entity.Repositories;

/// <summary>
/// Interface Repository chuyên biệt cho thực thể Vocabulary.
/// Cung cấp các thao tác truy vấn nâng cao kèm đầy đủ dữ liệu cụm Dictionary Cluster (Meanings & Examples).
/// </summary>
public interface IVocabularyRepository : IRepository<Vocabulary>
{
    /// <summary>
    /// Lấy ngẫu nhiên một từ vựng theo cấp độ CEFR kèm đầy đủ Meanings và Examples.
    /// Nếu cấp độ yêu cầu chưa có từ, tự động fallback lấy ngẫu nhiên từ bất kỳ cấp độ nào có sẵn.
    /// </summary>
    Task<Vocabulary?> GetRandomByLevelAsync(CEFRLevel level);

    /// <summary>
    /// Lấy thông tin chi tiết một từ vựng theo Id kèm đầy đủ Meanings và Examples.
    /// </summary>
    Task<Vocabulary?> GetWithDetailsAsync(Guid id);

    /// <summary>
    /// Lấy toàn bộ danh sách từ vựng kèm đầy đủ Meanings và Examples.
    /// </summary>
    Task<List<Vocabulary>> GetAllWithDetailsAsync();
}
