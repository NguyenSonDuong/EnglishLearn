using Microsoft.EntityFrameworkCore;
using English.Entity.Entities;
using English.Entity.Enums;
using English.Entity.Repositories;
using English.Repository.Data;

namespace English.Repository.Repositories;

/// <summary>
/// Triển khai Repository chuyên biệt cho thực thể Vocabulary.
/// Sử dụng EF Core với Eager Loading (.Include) để tải trọn vẹn cụm quan hệ Dictionary Cluster.
/// </summary>
public class VocabularyRepository : Repository<Vocabulary>, IVocabularyRepository
{
    private readonly AppDbContext _dbContext;

    public VocabularyRepository(AppDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Lấy ngẫu nhiên một từ vựng theo cấp độ CEFR kèm đầy đủ Meanings và Examples.
    /// Nếu cấp độ yêu cầu chưa có dữ liệu trong DB, fallback lấy ngẫu nhiên từ bất kỳ cấp độ nào có sẵn.
    /// </summary>
    public async Task<Vocabulary?> GetRandomByLevelAsync(CEFRLevel level)
    {
        // 1. Lọc từ theo cấp độ CEFR chỉ định
        var query = _dbContext.Vocabularies
            .Include(v => v.Meanings)
                .ThenInclude(m => m.Examples)
            .Where(v => v.Level == level);

        var count = await query.CountAsync();

        // 2. Nếu cấp độ này chưa có từ vựng, fallback sang toàn bộ DB
        if (count == 0)
        {
            query = _dbContext.Vocabularies
                .Include(v => v.Meanings)
                    .ThenInclude(m => m.Examples);

            count = await query.CountAsync();
            if (count == 0)
            {
                return null; // DB hoàn toàn chưa có từ nào
            }
        }

        // 3. Chọn ngẫu nhiên 1 phần tử
        var skip = Random.Shared.Next(0, count);
        return await query.Skip(skip).FirstOrDefaultAsync();
    }

    /// <summary>
    /// Lấy thông tin chi tiết một từ vựng theo Id kèm đầy đủ Meanings và Examples.
    /// </summary>
    public async Task<Vocabulary?> GetWithDetailsAsync(Guid id)
    {
        return await _dbContext.Vocabularies
            .Include(v => v.Meanings)
                .ThenInclude(m => m.Examples)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    /// <summary>
    /// Lấy toàn bộ danh sách từ vựng kèm đầy đủ Meanings và Examples.
    /// </summary>
    public async Task<List<Vocabulary>> GetAllWithDetailsAsync()
    {
        return await _dbContext.Vocabularies
            .Include(v => v.Meanings)
                .ThenInclude(m => m.Examples)
            .ToListAsync();
    }
}
