using English.Entity.Entities;

namespace English.Entity.Repositories;

/// <summary>
/// Interface repository cho câu hỏi (Question).
/// </summary>
public interface IQuestionRepository : IRepository<Question>
{
    /// <summary>Lấy câu hỏi theo LearningMaterialId.</summary>
    Task<List<Question>> GetByMaterialIdAsync(Guid materialId);

    /// <summary>Lấy câu hỏi đến hạn ôn tập (dựa trên StudyRecord.NextReviewTime).</summary>
    Task<List<Question>> GetDueQuestionsAsync(DateTime dueBeforeUtc);

    /// <summary>Lấy ngẫu nhiên N câu hỏi.</summary>
    Task<List<Question>> GetRandomQuestionsAsync(int count);
}
