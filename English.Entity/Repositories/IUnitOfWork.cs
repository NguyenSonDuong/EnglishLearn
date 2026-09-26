using English.Entity.Entities;

namespace English.Entity.Repositories;

/// <summary>
/// Unit of Work Interface điều phối các Repository và quản lý transaction.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IVocabularyRepository Vocabularies { get; }
    IRepository<VocabularyMeaning> Meanings { get; }
    IRepository<MeaningExample> Examples { get; }

    Task<int> SaveChangesAsync();
}
