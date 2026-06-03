using FastBiteGroup.Domain.Abstractions;
using FastBiteGroup.Domain.Abstractions.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FastBiteGroup.Persistence.Repositories;

/// <summary>
/// Generic EF Core repository base implementing IRepositoryBase.
/// All write operations (Add/Update/Remove) only stage changes — call IUnitOfWork.SaveChangesAsync to persist.
/// Read operations use AsNoTracking for query-only paths.
/// </summary>
public class RepositoryBase<TEntity, TKey> : IRepositoryBase<TEntity, TKey>
    where TEntity : EntityBase<TKey>
{
    private readonly ApplicationDbContext _dbContext;

    public RepositoryBase(ApplicationDbContext dbContext)
        => _dbContext = dbContext;

    /// <summary>
    /// Returns tracked or untracked queryable depending on caller intent.
    /// Internal helper — do NOT expose publicly.
    /// </summary>
    private IQueryable<TEntity> BuildQuery(
        Expression<Func<TEntity, bool>>? predicate,
        bool asNoTracking,
        Expression<Func<TEntity, object>>[] includeProperties)
    {
        IQueryable<TEntity> query = _dbContext.Set<TEntity>();

        if (asNoTracking)
            query = query.AsNoTracking();

        if (predicate is not null)
            query = query.Where(predicate);

        query = includeProperties.Aggregate(query,
            (current, include) => current.Include(include));

        return query;
    }

    /// <inheritdoc />
    public async Task<TEntity> FindByIdAsync(
        TKey id,
        CancellationToken cancellationToken = default,
        params Expression<Func<TEntity, object>>[] includeProperties)
    {
        var query = BuildQuery(null, asNoTracking: true, includeProperties);
        return await query.FirstOrDefaultAsync(e => e.Id!.Equals(id), cancellationToken)
               ?? throw new KeyNotFoundException($"{typeof(TEntity).Name} with id '{id}' was not found.");
    }

    /// <inheritdoc />
    public async Task<TEntity> FindSingleAsync(
        Expression<Func<TEntity, bool>>? predicate,
        CancellationToken cancellationToken = default,
        params Expression<Func<TEntity, object>>[] includeProperties)
    {
        var query = BuildQuery(predicate, asNoTracking: true, includeProperties);
        return await query.FirstOrDefaultAsync(cancellationToken)
               ?? throw new KeyNotFoundException($"{typeof(TEntity).Name} matching predicate was not found.");
    }

    /// <inheritdoc />
    public IQueryable<TEntity> FindAll(
        Expression<Func<TEntity, bool>>? predicate = null,
        params Expression<Func<TEntity, object>>[] includeProperties)
        => BuildQuery(predicate, asNoTracking: true, includeProperties);

    /// <inheritdoc />
    public void Add(TEntity entity)
        => _dbContext.Set<TEntity>().Add(entity);

    /// <inheritdoc />
    public void Update(TEntity entity)
        => _dbContext.Set<TEntity>().Update(entity);

    /// <inheritdoc />
    public void Remove(TEntity entity)
        => _dbContext.Set<TEntity>().Remove(entity);

    /// <inheritdoc />
    public void RemoveMultiple(List<TEntity> entities)
        => _dbContext.Set<TEntity>().RemoveRange(entities);
}
