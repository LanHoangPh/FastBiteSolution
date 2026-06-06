namespace FastBiteGroup.Persistence.Mongo.Documents;

public sealed class NotificationDocument : MongoDocumentBase<ObjectId>
{
    [BsonElement("user_id")]
    [BsonRepresentation(BsonType.String)]
    public Guid UserId { get; set; }

    [BsonElement("type")]
    [BsonRepresentation(BsonType.String)]
    public EnumNotificationType Type { get; set; }

    [BsonElement("content_preview")]
    public string ContentPreview { get; set; } = string.Empty;

    [BsonElement("is_read")]
    public bool IsRead { get; set; }

    [BsonElement("read_at")]
    [BsonIgnoreIfNull]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? ReadAt { get; set; }

    [BsonElement("created_at")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; }

    [BsonElement("deleted_at")]
    [BsonIgnoreIfNull]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? DeletedAt { get; set; }

    [BsonElement("related_object")]
    public RelatedObjectInfoDocument? RelatedObject { get; set; }
}
