using English.Entity.Entities;

namespace English.Entity.Repositories;

/// <summary>
/// Interface repository cho bản ghi ôn tập (StudyRecord).
/// </summary>
public interface IStudyRecordRepository : IRepository<StudyRecord>
{
    /// <summary>Lấy bản ghi ôn tập theo QuestionId (quan hệ 1-1).</summary>
    Task<StudyRecord?> GetByQuestionIdAsync(Guid questionId);
}
