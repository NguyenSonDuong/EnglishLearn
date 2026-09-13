using English.Entity.DTOs;

namespace English.Entity.Services;

/// <summary>Service quản lý câu hỏi.</summary>
public interface IQuestionService
{
    Task<List<QuestionDto>> GetByMaterialIdAsync(Guid materialId);
    Task<QuestionDto?> GetByIdAsync(Guid id);
    Task<List<QuestionDto>> GetDueQuestionsAsync();
    Task<List<QuestionDto>> GetRandomQuestionsAsync(int count);
    Task<QuestionDto> CreateAsync(QuestionDto dto);
    Task UpdateAsync(QuestionDto dto);
    Task DeleteAsync(Guid id);
}
