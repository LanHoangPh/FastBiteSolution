namespace FastBiteGroup.Domain.Abstractions;

public interface IUnitOfWork : IAsyncDisposable
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<TResponse> ExecuteTransactionAsync<TResponse>(
        Func<Task<TResponse>> action,
        CancellationToken cancellationToken = default);
}
