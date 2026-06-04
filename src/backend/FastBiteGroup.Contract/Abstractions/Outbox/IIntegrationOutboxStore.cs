namespace FastBiteGroup.Contract.Abstractions.Outbox;

public interface IIntegrationOutboxStore
{
    Task AddAsync(IntegrationOutboxMessage message, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<IntegrationOutboxMessage>> GetPendingAsync(
        int batchSize,
        CancellationToken cancellationToken = default);

    Task MarkProcessedAsync(Guid id, CancellationToken cancellationToken = default);

    Task MarkFailedAsync(
        Guid id,
        string error,
        CancellationToken cancellationToken = default);
}
