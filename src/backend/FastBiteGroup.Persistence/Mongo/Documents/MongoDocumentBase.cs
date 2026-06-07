namespace FastBiteGroup.Persistence.Mongo.Documents;

public abstract class MongoDocumentBase<TKey>
{
    [BsonId]
    public TKey Id { get; init; } = default!;
}

public abstract class MongoGuidDocumentBase
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; init; }
}
