using English.Entity.DTOs;
using English.Entity.Enums;

namespace English.Entity.Services;

/// <summary>
/// Service tạo hoặc lấy dữ liệu từ vựng theo cấp độ CEFR.
/// Đóng vai trò abstraction để dễ dàng thay thế giữa Mock data, các AI Service (OpenAI, Claude, v.v.), hoặc từ điển offline.
/// </summary>
public interface IVocabularyGeneratorService
{
    /// <summary>
    /// Tạo hoặc lấy một VocabularyDto theo đúng cấp độ CEFR yêu cầu.
    /// </summary>
    /// <param name="level">Cấp độ CEFR (A1 → C2).</param>
    /// <returns>VocabularyDto hợp lệ nếu thành công, null nếu không thể tạo dữ liệu.</returns>
    Task<VocabularyDto?> GenerateVocabularyAsync(CEFRLevel level);
}
