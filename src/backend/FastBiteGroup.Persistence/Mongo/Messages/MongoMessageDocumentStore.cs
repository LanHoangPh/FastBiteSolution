using FastBiteGroup.Persistence.DependencyInjection.Options;
using FastBiteGroup.Persistence.Mongo.Documents;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FastBiteGroup.Persistence.Mongo.Messages;

public sealed class MongoMessageDocumentStore(MongoDbContext context, IOptions<MongoDbOptions> options)
    : IMessageDocumentStore
{
    private readonly IMongoCollection<MessageDocument> _collection = context.GetCollection<MessageDocument>(
        options.Value.MessagesCollectionName);

    public Task AddAsync(MessageDocument message, CancellationToken cancellationToken = default)
        => _collection.InsertOneAsync(message, cancellationToken: cancellationToken);

    public async Task<MessageDocument?> GetByIdAsync(
        ObjectId id,
        CancellationToken cancellationToken = default)
        => await _collection
            .Find(x => x.Id == id && !x.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<MessageDocument>> GetByConversationAsync(
        int conversationId,
        int limit,
        DateTime? beforeSentAt = null,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<MessageDocument>.Filter.Eq(x => x.ConversationId, conversationId)
            & Builders<MessageDocument>.Filter.Eq(x => x.IsDeleted, false);

        if (beforeSentAt is not null)
        {
            filter &= Builders<MessageDocument>.Filter.Lt(x => x.SentAt, beforeSentAt.Value);
        }

        return await _collection
            .Find(filter)
            .SortByDescending(x => x.SentAt)
            .Limit(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateContentAsync(
        ObjectId id,
        string content,
        DateTime editedAt,
        CancellationToken cancellationToken = default)
    {
        var update = Builders<MessageDocument>.Update
            .Set(x => x.Content, content)
            .Set(x => x.EditedAt, editedAt);

        await _collection.UpdateOneAsync(
            x => x.Id == id && !x.IsDeleted,
            update,
            cancellationToken: cancellationToken);
    }

    public async Task MarkDeletedAsync(
        ObjectId id,
        Guid deletedByUserId,
        DateTime deletedAt,
        CancellationToken cancellationToken = default)
    {
        var update = Builders<MessageDocument>.Update
            .Set(x => x.IsDeleted, true)
            .Set(x => x.DeletedAt, deletedAt)
            .Set(x => x.DeletedByUserId, deletedByUserId);

        await _collection.UpdateOneAsync(
            x => x.Id == id && !x.IsDeleted,
            update,
            cancellationToken: cancellationToken);
    }
}
