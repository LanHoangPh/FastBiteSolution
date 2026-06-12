using FastBiteGroup.Persistence.DependencyInjection.Options;
using FastBiteGroup.Persistence.Mongo.Documents;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FastBiteGroup.Persistence.Mongo.Notifications;

public sealed class MongoNotificationDocumentStore(MongoDbContext context, IOptions<MongoDbOptions> options)
    : INotificationDocumentStore
{
    private readonly IMongoCollection<NotificationDocument> _collection = context.GetCollection<NotificationDocument>(
        options.Value.NotificationsCollectionName);

    public Task AddAsync(
        NotificationDocument notification,
        CancellationToken cancellationToken = default)
        => _collection.InsertOneAsync(notification, cancellationToken: cancellationToken);

    public async Task<NotificationDocument?> GetByIdAsync(
        ObjectId id,
        CancellationToken cancellationToken = default)
        => await _collection
            .Find(x => x.Id == id && x.DeletedAt == null)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<NotificationDocument>> GetByUserAsync(
        Guid userId,
        int limit,
        DateTime? beforeCreatedAt = null,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<NotificationDocument>.Filter.Eq(x => x.UserId, userId)
            & Builders<NotificationDocument>.Filter.Eq(x => x.DeletedAt, null);

        if (beforeCreatedAt is not null)
        {
            filter &= Builders<NotificationDocument>.Filter.Lt(x => x.CreatedAt, beforeCreatedAt.Value);
        }

        return await _collection
            .Find(filter)
            .SortByDescending(x => x.CreatedAt)
            .Limit(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<NotificationDocument>> GetUnreadByUserAsync(
        Guid userId,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<NotificationDocument>.Filter.Eq(x => x.UserId, userId)
            & Builders<NotificationDocument>.Filter.Eq(x => x.IsRead, false)
            & Builders<NotificationDocument>.Filter.Eq(x => x.DeletedAt, null);

        return await _collection
            .Find(filter)
            .SortByDescending(x => x.CreatedAt)
            .Limit(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkReadAsync(
        ObjectId id,
        DateTime readAt,
        CancellationToken cancellationToken = default)
    {
        var update = Builders<NotificationDocument>.Update
            .Set(x => x.IsRead, true)
            .Set(x => x.ReadAt, readAt);

        await _collection.UpdateOneAsync(
            x => x.Id == id && x.DeletedAt == null,
            update,
            cancellationToken: cancellationToken);
    }

    public async Task MarkDeletedAsync(
        ObjectId id,
        DateTime deletedAt,
        CancellationToken cancellationToken = default)
    {
        var update = Builders<NotificationDocument>.Update
            .Set(x => x.DeletedAt, deletedAt);

        await _collection.UpdateOneAsync(
            x => x.Id == id && x.DeletedAt == null,
            update,
            cancellationToken: cancellationToken);
    }
}
