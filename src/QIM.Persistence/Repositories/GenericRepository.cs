using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using QIM.Application.Interfaces.Repositories;
using QIM.Domain.Common;
using QIM.Persistence.Contexts;

namespace QIM.Persistence.Repositories;

/// <summary>
/// Generic repository implementation using EF Core.
/// </summary>
public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    protected readonly QimDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(QimDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    /// <summary>
    /// Applies Include expressions, stripping Convert nodes that the compiler
    /// inserts for Expression&lt;Func&lt;T, object&gt;&gt; (collections, nullable refs).
    /// </summary>
    private static IQueryable<T> ApplyIncludes(IQueryable<T> query, Expression<Func<T, object>>[] includes)
    {
        foreach (var include in includes)
        {
            var body = include.Body;
            if (body is UnaryExpression { NodeType: ExpressionType.Convert } unary)
                body = unary.Operand;

            if (body is MemberExpression member)
                query = query.Include(member.Member.Name);
            else
                query = query.Include(include);
        }
        return query;
    }

    public async Task<T?> GetByIdAsync(int id) =>
        await _dbSet.FindAsync(id);

    public async Task<List<T>> GetAllAsync() =>
        await _dbSet.ToListAsync();

    public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>> predicate) =>
        await _dbSet.Where(predicate).ToListAsync();

    public async Task<List<T>> GetAllOrderedAsync<TKey>(
        Expression<Func<T, TKey>> orderBy, bool descending = false)
    {
        return descending
            ? await _dbSet.OrderByDescending(orderBy).ToListAsync()
            : await _dbSet.OrderBy(orderBy).ToListAsync();
    }

    public async Task<(List<T> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        Expression<Func<T, object>>? orderBy = null,
        bool descending = false,
        params Expression<Func<T, object>>[] includes)
    {
        // DEF-NEW-008: defensively clamp pagination so a hostile client cannot crash the API
        // (page=-1 used to cause a negative SQL OFFSET → 500).
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 1;
        if (pageSize > 100) pageSize = 100;

        IQueryable<T> query = _dbSet;

        if (predicate is not null)
            query = query.Where(predicate);

        query = ApplyIncludes(query, includes);

        var totalCount = await query.CountAsync();

        if (orderBy is not null)
            query = descending ? query.OrderByDescending(orderBy) : query.OrderBy(orderBy);
        else
            query = query.OrderBy(e => e.Id);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate,
        params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = ApplyIncludes(_dbSet, includes);
        return await query.FirstOrDefaultAsync(predicate);
    }

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IQueryable<T>> queryBuilder)
    {
        IQueryable<T> query = queryBuilder(_dbSet);
        return await query.FirstOrDefaultAsync(predicate);
    }

    public async Task<List<T>> GetAllAsync(
        Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy,
        params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet.Where(predicate);
        query = ApplyIncludes(query, includes);
        if (orderBy is not null)
            query = orderBy(query);
        return await query.ToListAsync();
    }

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate) =>
        await _dbSet.AnyAsync(predicate);

    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null) =>
        predicate is null ? await _dbSet.CountAsync() : await _dbSet.CountAsync(predicate);

    public async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public void Update(T entity) =>
        _dbSet.Update(entity);

    public void Delete(T entity) =>
        _dbSet.Remove(entity);

    public void SoftDelete(T entity) =>
        entity.IsDeleted = true;
}
