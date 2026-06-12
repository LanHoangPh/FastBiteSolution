using FastBiteGroup.Domain.Abstractions;

namespace FastBiteGroup.Persistence;

public sealed class EfUnitOfWork(ApplicationDbContext context) : IUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);

    public async Task<TResponse> ExecuteTransactionAsync<TResponse>(
        Func<Task<TResponse>> action,
        CancellationToken cancellationToken = default)
    {
        var strategy = context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var result = await action();
                await context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return result;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
        => context.DisposeAsync();
}
