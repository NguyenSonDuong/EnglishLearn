using Microsoft.EntityFrameworkCore;
using English.Repository.Data;
using English.Entity.Entities;
using English.Entity.Repositories;

namespace English.Repository.Repositories;

/// <summary>
/// Triển khai repository cho bản ghi ôn tập (StudyRecord).
/// </summary>
public class StudyRecordRepository : Repository<StudyRecord>, IStudyRecordRepository
{
    public StudyRecordRepository(AppDbContext context) : base(context) { }

    public async Task<StudyRecord?> GetByQuestionIdAsync(Guid questionId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(sr => sr.QuestionId == questionId);
    }
}
