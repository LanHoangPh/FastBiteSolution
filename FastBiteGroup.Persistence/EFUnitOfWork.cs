using FastBiteGroup.Domain.Abstractions;

namespace FastBiteGroup.Persistence;

public class EFUnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public EFUnitOfWork(ApplicationDbContext context)
    => _context = context;

    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }

    public Task<TResponse> ExecuteTransactionAsync<TResponse>(Func<Task<TResponse>> action, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
