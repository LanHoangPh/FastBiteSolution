using FastBiteGroup.Persistence.Mongo.Documents;
using MongoDB.Bson;

namespace FastBiteGroup.Persistence.Mongo.Messages;

public interface IMessageDocumentStore
{
    Task AddAsync(MessageDocument message, CancellationToken cancellationToken = default);

    Task<MessageDocument?> GetByIdAsync(ObjectId id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MessageDocument>> GetByConversationAsync(
        int conversationId,
        int limit,
        DateTime? beforeSentAt = null,
        CancellationToken cancellationToken = default);

    Task UpdateContentAsync(
        ObjectId id,
        string content,
        DateTime editedAt,
        CancellationToken cancellationToken = default);

    Task MarkDeletedAsync(
        ObjectId id,
        Guid deletedByUserId,
        DateTime deletedAt,
        CancellationToken cancellationToken = default);
}
