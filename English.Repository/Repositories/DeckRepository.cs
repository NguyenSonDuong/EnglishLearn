using Microsoft.EntityFrameworkCore;
using English.Repository.Data;
using English.Entity.Entities;
using English.Entity.Repositories;

namespace English.Repository.Repositories;

/// <summary>
/// Triển khai repository cho bộ thẻ (Deck).
/// </summary>
public class DeckRepository : Repository<Deck>, IDeckRepository
{
    public DeckRepository(AppDbContext context) : base(context) { }

    public async Task<List<Deck>> GetActiveDecksAsync()
    {
        return await _dbSet.Where(d => d.IsActive).ToListAsync();
    }
}
