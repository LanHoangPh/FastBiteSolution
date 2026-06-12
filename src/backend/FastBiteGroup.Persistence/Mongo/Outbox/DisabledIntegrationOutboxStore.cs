using FastBiteGroup.Contract.Abstractions.Outbox;
using Microsoft.Extensions.Logging;

namespace FastBiteGroup.Persistence.Mongo.Outbox;

public sealed class DisabledIntegrationOutboxStore(ILogger<DisabledIntegrationOutboxStore> logger)
    : IIntegrationOutboxStore
{
    public Task AddAsync(
        IntegrationOutboxMessage message,
        CancellationToken cancellationToken = default)
    {
        logger.LogWarning(
            "Integration outbox is disabled. Message {MessageId} of type {MessageType} was not stored.",
            message.Id,
            message.Type);

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<IntegrationOutboxMessage>> GetPendingAsync(
        int batchSize,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<IntegrationOutboxMessage>>([]);

    public Task MarkProcessedAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task MarkFailedAsync(
        Guid id,
        string error,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
