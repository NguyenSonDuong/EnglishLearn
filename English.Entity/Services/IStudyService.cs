using English.Entity.DTOs;

namespace English.Entity.Services;

/// <summary>Service quản lý ôn tập và lịch sử học tập.</summary>
public interface IStudyService
{
    Task<StudyRecordDto?> GetRecordByQuestionIdAsync(Guid questionId);
    Task UpsertRecordAsync(StudyRecordDto dto);
    Task LogHistoryAsync(StudyHistoryDto dto);
    Task<List<StudyHistoryDto>> GetHistoryByQuestionIdAsync(Guid questionId);
}
