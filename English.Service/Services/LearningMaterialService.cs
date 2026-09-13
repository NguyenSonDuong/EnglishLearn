using English.Entity.DTOs;
using English.Entity.Entities;
using English.Entity.Repositories;
using English.Entity.Services;

namespace English.Service.Services;

public class LearningMaterialService : ILearningMaterialService
{
    private readonly ILearningMaterialRepository _repository;

    public LearningMaterialService(ILearningMaterialRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<LearningMaterialDto>> GetByDeckIdAsync(Guid deckId)
    {
        var entities = await _repository.GetByDeckIdAsync(deckId);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<LearningMaterialDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity is null ? null : MapToDto(entity);
    }

    public async Task<LearningMaterialDto> CreateAsync(LearningMaterialDto dto)
    {
        var entity = new LearningMaterial
        {
            Id = Guid.NewGuid(),
            DeckId = dto.DeckId,
            Term = dto.Term,
            Meaning = dto.Meaning,
            CategoryType = dto.CategoryType,
            ContextTag = dto.ContextTag,
            Phonetics = dto.Phonetics,
            ExampleSentence = dto.ExampleSentence
        };
        await _repository.AddAsync(entity);
        await _repository.SaveChangesAsync();
        return MapToDto(entity);
    }

    public async Task UpdateAsync(LearningMaterialDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException($"LearningMaterial {dto.Id} không tồn tại.");
        entity.DeckId = dto.DeckId;
        entity.Term = dto.Term;
        entity.Meaning = dto.Meaning;
        entity.CategoryType = dto.CategoryType;
        entity.ContextTag = dto.ContextTag;
        entity.Phonetics = dto.Phonetics;
        entity.ExampleSentence = dto.ExampleSentence;
        _repository.Update(entity);
        await _repository.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"LearningMaterial {id} không tồn tại.");
        _repository.Delete(entity);
        await _repository.SaveChangesAsync();
    }

    private static LearningMaterialDto MapToDto(LearningMaterial entity) => new()
    {
        Id = entity.Id,
        DeckId = entity.DeckId,
        Term = entity.Term,
        Meaning = entity.Meaning,
        CategoryType = entity.CategoryType,
        ContextTag = entity.ContextTag,
        Phonetics = entity.Phonetics,
        ExampleSentence = entity.ExampleSentence,
        QuestionCount = entity.Questions?.Count ?? 0
    };
}
