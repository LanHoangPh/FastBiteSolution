using FastBiteGroup.Persistence.DependencyInjection.Options;
using FastBiteGroup.Persistence.Mongo.Documents;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FastBiteGroup.Persistence.Mongo;

public sealed class MongoIndexInitializer : IHostedService
{
    private readonly IMongoCollection<MongoOutboxMessageDocument> _outboxCollection;

    public MongoIndexInitializer(MongoDbContext context, IOptions<MongoDbOptions> options)
        => _outboxCollection = context.GetCollection<MongoOutboxMessageDocument>(
            options.Value.OutboxCollectionName);

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var statusOccurredAtIndex = new CreateIndexModel<MongoOutboxMessageDocument>(
            Builders<MongoOutboxMessageDocument>.IndexKeys
                .Ascending(x => x.Status)
                .Ascending(x => x.OccurredAt));

        await _outboxCollection.Indexes.CreateOneAsync(
            statusOccurredAtIndex,
            cancellationToken: cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}
