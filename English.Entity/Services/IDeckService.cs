using English.Entity.DTOs;

namespace English.Entity.Services;

/// <summary>Service quản lý Deck.</summary>
public interface IDeckService
{
    Task<List<DeckDto>> GetAllDecksAsync();
    Task<DeckDto?> GetDeckByIdAsync(Guid id);
    Task<DeckDto> CreateDeckAsync(DeckDto dto);
    Task UpdateDeckAsync(DeckDto dto);
    Task DeleteDeckAsync(Guid id);
    Task<List<DeckDto>> GetActiveDecksAsync();
}
