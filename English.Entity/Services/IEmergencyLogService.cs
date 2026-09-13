using English.Entity.DTOs;

namespace English.Entity.Services;

public interface IEmergencyLogService
{
    Task<EmergencyLogDto> LogBypassAsync(EmergencyLogDto dto);
    Task<List<EmergencyLogDto>> GetAllAsync();
}
