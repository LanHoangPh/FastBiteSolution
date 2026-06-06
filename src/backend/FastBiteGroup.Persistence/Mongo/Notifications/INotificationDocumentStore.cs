using FastBiteGroup.Persistence.Mongo.Documents;
using MongoDB.Bson;

namespace FastBiteGroup.Persistence.Mongo.Notifications;

public interface INotificationDocumentStore
{
    Task AddAsync(NotificationDocument notification, CancellationToken cancellationToken = default);

    Task<NotificationDocument?> GetByIdAsync(ObjectId id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NotificationDocument>> GetByUserAsync(
        Guid userId,
        int limit,
        DateTime? beforeCreatedAt = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NotificationDocument>> GetUnreadByUserAsync(
        Guid userId,
        int limit,
        CancellationToken cancellationToken = default);

    Task MarkReadAsync(
        ObjectId id,
        DateTime readAt,
        CancellationToken cancellationToken = default);

    Task MarkDeletedAsync(
        ObjectId id,
        DateTime deletedAt,
        CancellationToken cancellationToken = default);
}
