using English.Entity.Entities;

namespace English.Entity.Repositories;

/// <summary>
/// Interface repository cho bộ thẻ (Deck).
/// </summary>
public interface IDeckRepository : IRepository<Deck>
{
    /// <summary>Lấy danh sách deck đang active.</summary>
    Task<List<Deck>> GetActiveDecksAsync();
}
