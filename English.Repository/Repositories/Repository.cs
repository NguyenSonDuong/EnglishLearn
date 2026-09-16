using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using English.Entity.Repositories;
using English.Repository.Data;

namespace English.Repository.Repositories;

/// <summary>
/// Triển khai Generic Repository cho Entity Framework Core.
/// </summary>
/// <typeparam name="T">Kiểu thực thể dữ liệu.</typeparam>
public class Repository<T> : IRepository<T> where T : class
{
    private readonly AppDbContext _dbContext;

    public Repository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Set<T>().FindAsync(id);
    }

    public virtual async Task<List<T>> GetAllAsync()
    {
        return await _dbContext.Set<T>().ToListAsync();
    }

    public virtual async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbContext.Set<T>().Where(predicate).ToListAsync();
    }

    public virtual async Task AddAsync(T entity)
    {
        await _dbContext.Set<T>().AddAsync(entity);
    }

    public virtual void Update(T entity)
    {
        _dbContext.Set<T>().Update(entity);
    }

    public virtual void Delete(T entity)
    {
        _dbContext.Set<T>().Remove(entity);
    }
}
