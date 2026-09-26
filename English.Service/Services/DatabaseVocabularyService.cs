using AutoMapper;
using Microsoft.Extensions.Logging;
using English.Entity.DTOs;
using English.Entity.Enums;
using English.Entity.Repositories;
using English.Entity.Services;

namespace English.Service.Services;

/// <summary>
/// Triển khai dịch vụ sinh từ vựng kết nối trực tiếp với cơ sở dữ liệu SQLite (englishlocker.db).
/// Tuân thủ nguyên lý Dependency Inversion, chỉ phụ thuộc vào IUnitOfWork và IMapper từ English.Entity.
/// Tích hợp cơ chế Fallback sang MockVocabularyService khi cơ sở dữ liệu chưa có dữ liệu.
/// </summary>
public class DatabaseVocabularyService : IVocabularyGeneratorService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<DatabaseVocabularyService> _logger;
    private readonly MockVocabularyService _mockFallbackService;

    public DatabaseVocabularyService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<DatabaseVocabularyService>? logger = null)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<DatabaseVocabularyService>.Instance;
        _mockFallbackService = new MockVocabularyService();
    }

    /// <summary>
    /// Lấy một từ vựng theo cấp độ CEFR từ Database (kèm trọn vẹn cụm Meanings & Examples).
    /// Nếu cơ sở dữ liệu trống, tự động fallback sang dữ liệu mock an toàn.
    /// </summary>
    public async Task<VocabularyDto?> GenerateVocabularyAsync(CEFRLevel level)
    {
        try
        {
            _logger.LogInformation("Đang truy vấn từ vựng cấp độ {Level} từ cơ sở dữ liệu SQLite...", level);

            // 1. Truy vấn từ cơ sở dữ liệu thông qua Repository chuyên biệt
            var entity = await _unitOfWork.Vocabularies.GetRandomByLevelAsync(level);

            if (entity != null)
            {
                _logger.LogInformation("Tìm thấy từ vựng '{WordText}' (Level: {Level}) từ cơ sở dữ liệu.", entity.WordText, entity.Level);
                return _mapper.Map<VocabularyDto>(entity);
            }

            _logger.LogWarning("Không tìm thấy từ vựng nào trong cơ sở dữ liệu cho cấp độ {Level}. Kích hoạt chế độ Mock Fallback.", level);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi xảy ra khi truy vấn từ vựng từ cơ sở dữ liệu. Chuyển sang chế độ Mock Fallback.");
        }

        // Fallback an toàn nếu DB trống hoặc xảy ra lỗi truy vấn
        return await _mockFallbackService.GenerateVocabularyAsync(level);
    }
}
