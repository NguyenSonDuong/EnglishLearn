using EnglishManager.Models;

namespace EnglishManager.Services;

/// <summary>
/// Interface giao tiếp với AI Dictionary API nội bộ (http://localhost:8000).
/// Tách interface để dễ mock/test và tuân thủ nguyên tắc Dependency Inversion.
/// </summary>
public interface IAiDictionaryApiService
{
    /// <summary>
    /// Gọi POST /api/vocabulary/generate
    /// </summary>
    /// <param name="level">Cấp độ CEFR: A1, A2, B1, B2, C1, C2 hoặc Random.</param>
    /// <param name="excludeWords">Danh sách từ tiếng Anh đã có trong database cần loại trừ.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<AiDictionaryResponse?> GenerateByLevelAsync(string level, List<string>? excludeWords = null, CancellationToken ct = default);

    /// <summary>
    /// Gọi POST /api/vocabulary/lookup
    /// </summary>
    /// <param name="word">Từ tiếng Anh cần tra cứu.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<AiDictionaryResponse?> LookupWordAsync(string word, CancellationToken ct = default);
}
