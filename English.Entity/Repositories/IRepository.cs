using System.Linq.Expressions;

namespace English.Entity.Repositories;

/// <summary>
/// Generic repository interface cho các thao tác CRUD bất đồng bộ cơ bản.
/// </summary>
/// <typeparam name="T">Kiểu thực thể dữ liệu.</typeparam>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<List<T>> GetAllAsync();
    Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
}
