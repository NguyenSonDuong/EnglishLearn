namespace English.Entity.Repositories;

/// <summary>
/// Generic repository interface cho các thao tác CRUD cơ bản.
/// </summary>
/// <typeparam name="T">Kiểu thực thể dữ liệu.</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>Lấy thực thể theo Id.</summary>
    Task<T?> GetByIdAsync(Guid id);

    /// <summary>Lấy tất cả thực thể.</summary>
    Task<List<T>> GetAllAsync();

    /// <summary>Thêm mới thực thể.</summary>
    Task AddAsync(T entity);

    /// <summary>Cập nhật thực thể.</summary>
    void Update(T entity);

    /// <summary>Xóa thực thể.</summary>
    void Delete(T entity);

    /// <summary>Lưu các thay đổi vào cơ sở dữ liệu.</summary>
    Task SaveChangesAsync();
}
