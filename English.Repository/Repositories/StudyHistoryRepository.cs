using Microsoft.EntityFrameworkCore;
using English.Repository.Data;
using English.Entity.Entities;
using English.Entity.Repositories;

namespace English.Repository.Repositories;

/// <summary>
/// Triển khai repository cho lịch sử học tập (StudyHistory).
/// </summary>
public class StudyHistoryRepository : Repository<StudyHistory>, IStudyHistoryRepository
{
    public StudyHistoryRepository(AppDbContext context) : base(context) { }

    public async Task<List<StudyHistory>> GetByQuestionIdAsync(Guid questionId)
    {
        return await _dbSet
            .Where(sh => sh.QuestionId == questionId)
            .OrderByDescending(sh => sh.TestedAt)
            .ToListAsync();
    }
}
