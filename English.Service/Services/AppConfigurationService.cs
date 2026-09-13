using English.Entity.DTOs;
using English.Entity.Entities;
using English.Entity.Repositories;
using English.Entity.Services;

namespace English.Service.Services;

public class AppConfigurationService : IAppConfigurationService
{
    private readonly IAppConfigurationRepository _repository;

    public AppConfigurationService(IAppConfigurationRepository repository)
    {
        _repository = repository;
    }

    public async Task<string?> GetValueAsync(string key)
    {
        var entity = await _repository.GetByKeyAsync(key);
        return entity?.ConfigValue;
    }

    public async Task SetValueAsync(string key, string value, string valueType = "string", string? description = null)
    {
        var config = new AppConfiguration
        {
            ConfigKey = key,
            ConfigValue = value,
            ValueType = valueType,
            Description = description
        };
        await _repository.SetAsync(config);
        await _repository.SaveChangesAsync();
    }
    
    public async Task<List<AppConfigurationDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Select(e => new AppConfigurationDto
        {
            ConfigKey = e.ConfigKey, 
            ConfigValue = e.ConfigValue,
            ValueType = e.ValueType,
            Description = e.Description
        }).ToList();
    }

    public async Task DeleteAsync(string key)
    {
        await _repository.DeleteAsync(key);
        await _repository.SaveChangesAsync();
    }
}
