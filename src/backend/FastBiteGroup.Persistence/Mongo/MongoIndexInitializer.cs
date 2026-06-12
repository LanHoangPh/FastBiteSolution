using FastBiteGroup.Persistence.DependencyInjection.Options;
using FastBiteGroup.Persistence.Mongo.Documents;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FastBiteGroup.Persistence.Mongo;

public sealed class MongoIndexInitializer(MongoDbContext context, IOptions<MongoDbOptions> options) : IHostedService
{
    private readonly IMongoCollection<MessageDocument> _messageCollection = context.GetCollection<MessageDocument>(
        options.Value.MessagesCollectionName);
    private readonly IMongoCollection<NotificationDocument> _notificationCollection = context.GetCollection<NotificationDocument>(
        options.Value.NotificationsCollectionName);
    private readonly IMongoCollection<MongoOutboxMessageDocument> _outboxCollection = context.GetCollection<MongoOutboxMessageDocument>(
        options.Value.OutboxCollectionName);

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var conversationSentAtIndex = new CreateIndexModel<MessageDocument>(
            Builders<MessageDocument>.IndexKeys
                .Ascending(x => x.ConversationId)
                .Descending(x => x.SentAt));

        var senderSentAtIndex = new CreateIndexModel<MessageDocument>(
            Builders<MessageDocument>.IndexKeys
                .Ascending("sender.userId")
                .Descending(x => x.SentAt));

        var messageDeletedIndex = new CreateIndexModel<MessageDocument>(
            Builders<MessageDocument>.IndexKeys
                .Ascending(x => x.IsDeleted));

        await _messageCollection.Indexes.CreateManyAsync(
            new[] { conversationSentAtIndex, senderSentAtIndex, messageDeletedIndex },
            cancellationToken: cancellationToken);

        var userCreatedAtIndex = new CreateIndexModel<NotificationDocument>(
            Builders<NotificationDocument>.IndexKeys
                .Ascending(x => x.UserId)
                .Descending(x => x.CreatedAt));

        var unreadUserCreatedAtIndex = new CreateIndexModel<NotificationDocument>(
            Builders<NotificationDocument>.IndexKeys
                .Ascending(x => x.UserId)
                .Ascending(x => x.IsRead)
                .Descending(x => x.CreatedAt));

        var notificationDeletedIndex = new CreateIndexModel<NotificationDocument>(
            Builders<NotificationDocument>.IndexKeys
                .Ascending(x => x.DeletedAt));

        await _notificationCollection.Indexes.CreateManyAsync(
            new[] { userCreatedAtIndex, unreadUserCreatedAtIndex, notificationDeletedIndex },
            cancellationToken: cancellationToken);

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
