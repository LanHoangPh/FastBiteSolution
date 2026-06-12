using FastBiteGroup.Domain.Enums;

namespace FastBiteGroup.Persistence.Mongo.Documents;

public sealed class AttachmentInfoDocument
{
    [BsonElement("file_id")]
    public int FileId { get; set; }

    [BsonElement("file_name")]
    public string FileName { get; set; } = string.Empty;

    [BsonElement("storage_url")]
    public string StorageUrl { get; set; } = string.Empty;

    [BsonElement("file_type")]
    public string FileType { get; set; } = string.Empty;

    [BsonElement("file_size")]
    public long FileSize { get; set; }
}

public sealed class ParentMessageInfoDocument
{
    [BsonElement("sender_name")]
    public string SenderName { get; set; } = string.Empty;

    [BsonElement("content_snippet")]
    public string ContentSnippet { get; set; } = string.Empty;
}

public sealed class ReactionDocument
{
    [BsonElement("user_id")]
    [BsonRepresentation(BsonType.String)]
    public Guid UserId { get; set; }

    [BsonElement("code")]
    public string ReactionCode { get; set; } = string.Empty;

    [BsonElement("reacted_at")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime ReactedAt { get; set; }
}

public sealed class ReadReceiptInfoDocument
{
    [BsonElement("user_id")]
    [BsonRepresentation(BsonType.String)]
    public Guid UserId { get; set; }

    [BsonElement("read_at")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime ReadAt { get; set; }
}

public sealed class RelatedObjectInfoDocument
{
    [BsonElement("type")]
    [BsonRepresentation(BsonType.String)]
    public EnumNotificationObjectType ObjectType { get; set; }

    [BsonElement("id")]
    public string? ObjectId { get; set; }

    [BsonElement("url")]
    public string NavigateUrl { get; set; } = string.Empty;
}

public sealed class SenderInfoDocument
{
    [BsonElement("userId")]
    [BsonRepresentation(BsonType.String)]
    public Guid UserId { get; set; }

    [BsonElement("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [BsonElement("avatarUrl")]
    public string? AvatarUrl { get; set; }
}
