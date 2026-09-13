using English.Entity.DTOs;
using English.Entity.Entities;
using English.Entity.Repositories;
using English.Entity.Services;

namespace English.Service.Services;

public class StudyService : IStudyService
{
    private readonly IStudyRecordRepository _recordRepo;
    private readonly IStudyHistoryRepository _historyRepo;

    public StudyService(IStudyRecordRepository recordRepo, IStudyHistoryRepository historyRepo)
    {
        _recordRepo = recordRepo;
        _historyRepo = historyRepo;
    }

    public async Task<StudyRecordDto?> GetRecordByQuestionIdAsync(Guid questionId)
    {
        var entity = await _recordRepo.GetByQuestionIdAsync(questionId);
        return entity is null ? null : MapRecordToDto(entity);
    }

    public async Task UpsertRecordAsync(StudyRecordDto dto)
    {
        var existing = await _recordRepo.GetByQuestionIdAsync(dto.QuestionId);
        if (existing is null)
        {
            var entity = new StudyRecord
            {
                Id = Guid.NewGuid(),
                QuestionId = dto.QuestionId,
                RepetitionCount = dto.RepetitionCount,
                EasinessFactor = dto.EasinessFactor,
                Interval = dto.Interval,
                NextReviewTime = dto.NextReviewTime,
                LastTestedAt = dto.LastTestedAt
            };
            await _recordRepo.AddAsync(entity);
        }
        else
        {
            existing.RepetitionCount = dto.RepetitionCount;
            existing.EasinessFactor = dto.EasinessFactor;
            existing.Interval = dto.Interval;
            existing.NextReviewTime = dto.NextReviewTime;
            existing.LastTestedAt = dto.LastTestedAt;
            _recordRepo.Update(existing);
        }
        await _recordRepo.SaveChangesAsync();
    }

    public async Task LogHistoryAsync(StudyHistoryDto dto)
    {
        var entity = new StudyHistory
        {
            Id = Guid.NewGuid(),
            QuestionId = dto.QuestionId,
            IsCorrect = dto.IsCorrect,
            ResponseTimeMs = dto.ResponseTimeMs,
            TestedAt = dto.TestedAt
        };
        await _historyRepo.AddAsync(entity);
        await _historyRepo.SaveChangesAsync();
    }

    public async Task<List<StudyHistoryDto>> GetHistoryByQuestionIdAsync(Guid questionId)
    {
        var entities = await _historyRepo.GetByQuestionIdAsync(questionId);
        return entities.Select(MapHistoryToDto).ToList();
    }

    private static StudyRecordDto MapRecordToDto(StudyRecord e) => new()
    {
        Id = e.Id,
        QuestionId = e.QuestionId,
        RepetitionCount = e.RepetitionCount,
        EasinessFactor = e.EasinessFactor,
        Interval = e.Interval,
        NextReviewTime = e.NextReviewTime,
        LastTestedAt = e.LastTestedAt
    };

    private static StudyHistoryDto MapHistoryToDto(StudyHistory e) => new()
    {
        Id = e.Id,
        QuestionId = e.QuestionId,
        IsCorrect = e.IsCorrect,
        ResponseTimeMs = e.ResponseTimeMs,
        TestedAt = e.TestedAt
    };
}
