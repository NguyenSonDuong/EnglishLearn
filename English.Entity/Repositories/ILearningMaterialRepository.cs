using English.Entity.Entities;

namespace English.Entity.Repositories;

/// <summary>
/// Interface repository cho tài liệu học tập (LearningMaterial).
/// </summary>
public interface ILearningMaterialRepository : IRepository<LearningMaterial>
{
    /// <summary>Lấy tài liệu theo DeckId.</summary>
    Task<List<LearningMaterial>> GetByDeckIdAsync(Guid deckId);
}
