using FastBiteGroup.Domain.Abstractions;
using FastBiteGroup.Domain.Abstractions.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FastBiteGroup.Persistence.Repositories;

public class RepositoryBase<TEntity, TKey> : IRepositoryBase<TEntity, TKey>
    where TEntity : EntityBase<TKey>
{
    private readonly ApplicationDbContext _dbContext;
    public RepositoryBase(ApplicationDbContext dbContext)
    => _dbContext = dbContext;

    public void Add(TEntity entity)
    {
        throw new NotImplementedException();
    }

    public IQueryable<TEntity> FindAll(Expression<Func<TEntity, bool>>? predicate = null, params Expression<Func<TEntity, object>>[] includeProperties)
    {
        throw new NotImplementedException();
    }

    public Task<TEntity> FindByIdAsync(TKey id, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includeProperties)
    {
        throw new NotImplementedException();
    }

    public Task<TEntity> FindSingleAsync(Expression<Func<TEntity, bool>>? predicate, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includeProperties)
    {
        throw new NotImplementedException();
    }

    public void Remove(TEntity entity)
    {
        throw new NotImplementedException();
    }

    public void RemoveMultiple(List<TEntity> entities)
    {
        throw new NotImplementedException();
    }

    public void Update(TEntity entity)
    {
        throw new NotImplementedException();
    }
}
