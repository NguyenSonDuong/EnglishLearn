using Microsoft.EntityFrameworkCore;
using English.Repository.Data;
using English.Entity.Entities;
using English.Entity.Repositories;

namespace English.Repository.Repositories;

/// <summary>
/// Triển khai repository cho cấu hình ứng dụng (AppConfiguration).
/// </summary>
public class AppConfigurationRepository : IAppConfigurationRepository
{
    private readonly AppDbContext _context;

    public AppConfigurationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AppConfiguration?> GetByKeyAsync(string key)
    {
        return await _context.AppConfigurations.FindAsync(key);
    }

    public async Task<List<AppConfiguration>> GetAllAsync()
    {
        return await _context.AppConfigurations.ToListAsync();
    }

    public async Task SetAsync(AppConfiguration config)
    {
        var existing = await _context.AppConfigurations.FindAsync(config.ConfigKey);
        if (existing is null)
        {
            await _context.AppConfigurations.AddAsync(config);
        }
        else
        {
            existing.ConfigValue = config.ConfigValue;
            existing.ValueType = config.ValueType;
            existing.Description = config.Description;
        }
    }

    public async Task DeleteAsync(string key)
    {
        var entity = await _context.AppConfigurations.FindAsync(key);
        if (entity is not null)
        {
            _context.AppConfigurations.Remove(entity);
        }
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
