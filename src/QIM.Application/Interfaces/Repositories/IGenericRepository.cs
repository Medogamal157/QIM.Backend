using System.Linq.Expressions;
using QIM.Domain.Common;

namespace QIM.Application.Interfaces.Repositories;

/// <summary>
/// Generic repository interface for CRUD + pagination.
/// </summary>
public interface IGenericRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id);
    Task<List<T>> GetAllAsync();
    Task<List<T>> GetAllAsync(Expression<Func<T, bool>> predicate);
    Task<List<T>> GetAllOrderedAsync<TKey>(Expression<Func<T, TKey>> orderBy, bool descending = false);

    Task<(List<T> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        Expression<Func<T, object>>? orderBy = null,
        bool descending = false,
        params Expression<Func<T, object>>[] includes);

    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate,
        params Expression<Func<T, object>>[] includes);

    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IQueryable<T>> queryBuilder);

    Task<List<T>> GetAllAsync(
        Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy,
        params Expression<Func<T, object>>[] includes);

    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

    Task<T> AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
    void SoftDelete(T entity);
}
