using English.Entity.Entities;
using English.Entity.Repositories;
using English.Repository.Data;

namespace English.Repository.Repositories;

/// <summary>
/// Triển khai Unit of Work điều phối các Repository và quản lý transaction.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _dbContext;

    public IRepository<Vocabulary> Vocabularies { get; }
    public IRepository<VocabularyMeaning> Meanings { get; }
    public IRepository<MeaningExample> Examples { get; }

    public UnitOfWork(AppDbContext dbContext)
    {
        _dbContext = dbContext;
        Vocabularies = new Repository<Vocabulary>(_dbContext);
        Meanings = new Repository<VocabularyMeaning>(_dbContext);
        Examples = new Repository<MeaningExample>(_dbContext);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _dbContext.SaveChangesAsync();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }
}
