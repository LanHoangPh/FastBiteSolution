using FastBiteGroup.Contract.Abstractions.Outbox;
using FastBiteGroup.Persistence.DependencyInjection.Options;
using FastBiteGroup.Persistence.Mongo.Documents;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FastBiteGroup.Persistence.Mongo.Outbox;

public sealed class MongoIntegrationOutboxStore(MongoDbContext context, IOptions<MongoDbOptions> options)
    : IIntegrationOutboxStore
{
    private readonly IMongoCollection<MongoOutboxMessageDocument> _collection = context.GetCollection<MongoOutboxMessageDocument>(
        options.Value.OutboxCollectionName);

    public async Task AddAsync(
        IntegrationOutboxMessage message,
        CancellationToken cancellationToken = default)
    {
        var document = new MongoOutboxMessageDocument
        {
            Id = message.Id,
            Type = message.Type,
            Payload = message.Payload,
            OccurredAt = message.OccurredAt,
            CorrelationId = message.CorrelationId
        };

        await _collection.InsertOneAsync(document, cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<IntegrationOutboxMessage>> GetPendingAsync(
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        var documents = await _collection
            .Find(x => x.Status == MongoOutboxStatuses.Pending)
            .SortBy(x => x.OccurredAt)
            .Limit(batchSize)
            .ToListAsync(cancellationToken);

        return documents
            .Select(x => new IntegrationOutboxMessage(
                x.Id,
                x.Type,
                x.Payload,
                x.OccurredAt,
                x.CorrelationId))
            .ToList();
    }

    public async Task MarkProcessedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var update = Builders<MongoOutboxMessageDocument>.Update
            .Set(x => x.Status, MongoOutboxStatuses.Processed)
            .Set(x => x.ProcessedAt, DateTimeOffset.UtcNow)
            .Set(x => x.LastError, null);

        await _collection.UpdateOneAsync(
            x => x.Id == id,
            update,
            cancellationToken: cancellationToken);
    }

    public async Task MarkFailedAsync(
        Guid id,
        string error,
        CancellationToken cancellationToken = default)
    {
        var update = Builders<MongoOutboxMessageDocument>.Update
            .Set(x => x.Status, MongoOutboxStatuses.Failed)
            .Set(x => x.LastError, error)
            .Set(x => x.LastAttemptAt, DateTimeOffset.UtcNow)
            .Inc(x => x.RetryCount, 1);

        await _collection.UpdateOneAsync(
            x => x.Id == id,
            update,
            cancellationToken: cancellationToken);
    }
}
