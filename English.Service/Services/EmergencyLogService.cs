using English.Entity.DTOs;
using English.Entity.Entities;
using English.Entity.Repositories;
using English.Entity.Services;

namespace English.Service.Services;

public class EmergencyLogService : IEmergencyLogService
{
    private readonly IEmergencyLogRepository _repository;

    public EmergencyLogService(IEmergencyLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<EmergencyLogDto> LogBypassAsync(EmergencyLogDto dto)
    {
        var entity = new EmergencyLog
        {
            Id = Guid.NewGuid(),
            BypassedAt = dto.BypassedAt,
            Reason = dto.Reason,
            IsDeducted = dto.IsDeducted
        };
        await _repository.AddAsync(entity);
        await _repository.SaveChangesAsync();
        return MapToDto(entity);
    }

    public async Task<List<EmergencyLogDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Select(MapToDto).ToList();
    }

    private static EmergencyLogDto MapToDto(EmergencyLog e) => new()
    {
        Id = e.Id,
        BypassedAt = e.BypassedAt,
        Reason = e.Reason,
        IsDeducted = e.IsDeducted
    };
}
