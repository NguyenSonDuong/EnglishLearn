using English.Entity.Entities;

namespace English.Entity.Repositories;

/// <summary>
/// Interface repository cho lịch sử học tập (StudyHistory).
/// </summary>
public interface IStudyHistoryRepository : IRepository<StudyHistory>
{
    /// <summary>Lấy lịch sử trả lời theo QuestionId.</summary>
    Task<List<StudyHistory>> GetByQuestionIdAsync(Guid questionId);
}
