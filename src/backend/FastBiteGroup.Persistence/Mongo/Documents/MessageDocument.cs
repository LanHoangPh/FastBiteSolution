using FastBiteGroup.Domain.Enums;

namespace FastBiteGroup.Persistence.Mongo.Documents;

public sealed class MessageDocument : MongoDocumentBase<ObjectId>
{
    [BsonElement("conversation_id")]
    public int ConversationId { get; set; }

    [BsonElement("sender")]
    public SenderInfoDocument? Sender { get; set; }

    [BsonElement("content")]
    [BsonIgnoreIfNull]
    public string Content { get; set; } = string.Empty;

    [BsonElement("message_type")]
    [BsonRepresentation(BsonType.String)]
    public EnumMessageType MessageType { get; set; } = EnumMessageType.Text;

    [BsonElement("sent_at")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime SentAt { get; set; }

    [BsonElement("edited_at")]
    [BsonIgnoreIfNull]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? EditedAt { get; set; }

    [BsonElement("parent_message_id")]
    [BsonIgnoreIfNull]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? ParentMessageId { get; set; }

    [BsonElement("is_deleted")]
    public bool IsDeleted { get; set; }

    [BsonElement("deleted_at")]
    [BsonIgnoreIfNull]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? DeletedAt { get; set; }

    [BsonElement("deleted_by_user_id")]
    [BsonIgnoreIfNull]
    [BsonRepresentation(BsonType.String)]
    public Guid? DeletedByUserId { get; set; }

    [BsonElement("parent_message")]
    [BsonIgnoreIfNull]
    public ParentMessageInfoDocument? ParentMessage { get; set; }

    [BsonElement("attachments")]
    [BsonIgnoreIfNull]
    public List<AttachmentInfoDocument>? Attachments { get; set; }

    [BsonElement("reactions")]
    [BsonIgnoreIfNull]
    public List<ReactionDocument>? Reactions { get; set; }

    [BsonElement("mentions")]
    [BsonIgnoreIfNull]
    public List<Guid>? MentionedUserIds { get; set; }

    [BsonElement("read_by")]
    [BsonIgnoreIfNull]
    public List<ReadReceiptInfoDocument>? ReadBy { get; set; }
}
