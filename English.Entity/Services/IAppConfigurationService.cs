using English.Entity.DTOs;

namespace English.Entity.Services;

public interface IAppConfigurationService
{
    Task<string?> GetValueAsync(string key);
    Task SetValueAsync(string key, string value, string valueType = "string", string? description = null);
    Task<List<AppConfigurationDto>> GetAllAsync();
    Task DeleteAsync(string key);
}
