using Microsoft.EntityFrameworkCore;
using English.Repository.Data;
using English.Entity.Entities;
using English.Entity.Repositories;

namespace English.Repository.Repositories;

/// <summary>
/// Triển khai repository cho tài liệu học tập (LearningMaterial).
/// </summary>
public class LearningMaterialRepository : Repository<LearningMaterial>, ILearningMaterialRepository
{
    public LearningMaterialRepository(AppDbContext context) : base(context) { }

    public async Task<List<LearningMaterial>> GetByDeckIdAsync(Guid deckId)
    {
        return await _dbSet
            .Where(lm => lm.DeckId == deckId)
            .Include(lm => lm.Questions)
            .ToListAsync();
    }
}
