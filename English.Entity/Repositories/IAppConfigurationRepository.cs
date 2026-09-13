using English.Entity.Entities;

namespace English.Entity.Repositories;

/// <summary>Repository cho AppConfiguration (PK là string ConfigKey).</summary>
public interface IAppConfigurationRepository
{
    /// <summary>Lấy cấu hình theo key.</summary>
    Task<AppConfiguration?> GetByKeyAsync(string key);

    /// <summary>Lấy tất cả cấu hình.</summary>
    Task<List<AppConfiguration>> GetAllAsync();

    /// <summary>Thêm mới hoặc cập nhật cấu hình.</summary>
    Task SetAsync(AppConfiguration config);

    /// <summary>Xóa cấu hình theo key.</summary>
    Task DeleteAsync(string key);

    /// <summary>Lưu các thay đổi vào cơ sở dữ liệu.</summary>
    Task SaveChangesAsync();
}
