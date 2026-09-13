using System.Text.Json;
using English.Entity.DTOs;
using English.Entity.Entities;
using English.Entity.Repositories;
using English.Entity.Services;

namespace English.Service.Services;

public class QuestionService : IQuestionService
{
    private readonly IQuestionRepository _repository;

    public QuestionService(IQuestionRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<QuestionDto>> GetByMaterialIdAsync(Guid materialId)
    {
        var entities = await _repository.GetByMaterialIdAsync(materialId);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<QuestionDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity is null ? null : MapToDto(entity);
    }

    public async Task<List<QuestionDto>> GetDueQuestionsAsync()
    {
        var entities = await _repository.GetDueQuestionsAsync(DateTime.UtcNow);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<List<QuestionDto>> GetRandomQuestionsAsync(int count)
    {
        var entities = await _repository.GetRandomQuestionsAsync(count);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<QuestionDto> CreateAsync(QuestionDto dto)
    {
        var entity = new Question
        {
            Id = Guid.NewGuid(),
            LearningMaterialId = dto.LearningMaterialId,
            TestType = dto.TestType,
            Prompt = dto.Prompt,
            CorrectAnswer = dto.CorrectAnswer,
            OptionsPayload = dto.Options.Count > 0
                ? JsonSerializer.Serialize(dto.Options)
                : null,
            AudioLocalPath = dto.AudioLocalPath,
            ImageLocalPath = dto.ImageLocalPath
        };
        await _repository.AddAsync(entity);
        await _repository.SaveChangesAsync();
        return MapToDto(entity);
    }

    public async Task UpdateAsync(QuestionDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException($"Question {dto.Id} không tồn tại.");
        entity.LearningMaterialId = dto.LearningMaterialId;
        entity.TestType = dto.TestType;
        entity.Prompt = dto.Prompt;
        entity.CorrectAnswer = dto.CorrectAnswer;
        entity.OptionsPayload = dto.Options.Count > 0
            ? JsonSerializer.Serialize(dto.Options)
            : null;
        entity.AudioLocalPath = dto.AudioLocalPath;
        entity.ImageLocalPath = dto.ImageLocalPath;
        _repository.Update(entity);
        await _repository.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Question {id} không tồn tại.");
        _repository.Delete(entity);
        await _repository.SaveChangesAsync();
    }

    private static QuestionDto MapToDto(Question entity) => new()
    {
        Id = entity.Id,
        LearningMaterialId = entity.LearningMaterialId,
        TestType = entity.TestType,
        Prompt = entity.Prompt,
        CorrectAnswer = entity.CorrectAnswer,
        Options = ParseOptions(entity.OptionsPayload),
        AudioLocalPath = entity.AudioLocalPath,
        ImageLocalPath = entity.ImageLocalPath
    };

    private static List<string> ParseOptions(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new List<string>();
        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }
}
