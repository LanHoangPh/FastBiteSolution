namespace FastBiteGroup.Persistence.Mongo.Documents;

public abstract class MongoDocumentBase<TKey>
{
    [BsonId]
    public TKey Id { get; init; } = default!;
}
