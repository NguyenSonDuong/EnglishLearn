using English.Entity.DTOs;

namespace English.Entity.Services;

/// <summary>
/// Giao diện Service xử lý nghiệp vụ và các thao tác CRUD cho câu ví dụ minh họa (MeaningExample).
/// Che giấu hoàn toàn các Entity bên trong, chỉ trao đổi qua DTO.
/// </summary>
public interface IMeaningExampleService
{
    Task<MeaningExampleDto?> GetExampleByIdAsync(Guid id);
    Task<List<MeaningExampleDto>> GetAllExamplesAsync();
    Task<List<MeaningExampleDto>> GetExamplesByMeaningIdAsync(Guid meaningId);
    Task<MeaningExampleDto?> CreateExampleAsync(MeaningExampleDto dto);
    Task<bool> UpdateExampleAsync(MeaningExampleDto dto);
    Task<bool> DeleteExampleAsync(Guid id);
}
