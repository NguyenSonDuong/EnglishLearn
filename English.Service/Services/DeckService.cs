using English.Entity.DTOs;
using English.Entity.Entities;
using English.Entity.Repositories;
using English.Entity.Services;

namespace English.Service.Services;

public class DeckService : IDeckService
{
    private readonly IDeckRepository _repository;

    public DeckService(IDeckRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<DeckDto>> GetAllDecksAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Select(MapToDto).ToList();
    }

    public async Task<DeckDto?> GetDeckByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity is null ? null : MapToDto(entity);
    }

    public async Task<DeckDto> CreateDeckAsync(DeckDto dto)
    {
        var entity = new Deck
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            IsActive = dto.IsActive
        };
        await _repository.AddAsync(entity);
        await _repository.SaveChangesAsync();
        return MapToDto(entity);
    }

    public async Task UpdateDeckAsync(DeckDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException($"Deck {dto.Id} không tồn tại.");
        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.IsActive = dto.IsActive;
        _repository.Update(entity);
        await _repository.SaveChangesAsync();
    }

    public async Task DeleteDeckAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Deck {id} không tồn tại.");
        _repository.Delete(entity);
        await _repository.SaveChangesAsync();
    }

    public async Task<List<DeckDto>> GetActiveDecksAsync()
    {
        var entities = await _repository.GetActiveDecksAsync();
        return entities.Select(MapToDto).ToList();
    }

    private static DeckDto MapToDto(Deck entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Description = entity.Description,
        IsActive = entity.IsActive,
        MaterialCount = entity.LearningMaterials?.Count ?? 0
    };
}
