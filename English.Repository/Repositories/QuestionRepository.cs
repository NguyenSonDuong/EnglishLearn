using Microsoft.EntityFrameworkCore;
using English.Repository.Data;
using English.Entity.Entities;
using English.Entity.Repositories;

namespace English.Repository.Repositories;

/// <summary>
/// Triển khai repository cho câu hỏi (Question).
/// </summary>
public class QuestionRepository : Repository<Question>, IQuestionRepository
{
    public QuestionRepository(AppDbContext context) : base(context) { }

    public async Task<List<Question>> GetByMaterialIdAsync(Guid materialId)
    {
        return await _dbSet
            .Where(q => q.LearningMaterialId == materialId)
            .ToListAsync();
    }

    public async Task<List<Question>> GetDueQuestionsAsync(DateTime dueBeforeUtc)
    {
        return await _dbSet
            .Include(q => q.StudyRecord)
            .Where(q => q.StudyRecord == null || q.StudyRecord.NextReviewTime <= dueBeforeUtc)
            .ToListAsync();
    }

    public async Task<List<Question>> GetRandomQuestionsAsync(int count)
    {
        // SQLite không hỗ trợ NEWID(), dùng Guid ordering thay thế
        return await _dbSet
            .Take(count)
            .ToListAsync();
    }
}
