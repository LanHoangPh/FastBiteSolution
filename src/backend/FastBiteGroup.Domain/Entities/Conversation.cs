using System.ComponentModel.DataAnnotations;

namespace FastBiteGroup.Domain.Entities;

public class Conversation : IAuditable
{
    public int ConversationID { get; set; }
    public EnumConversationType ConversationType { get; set; }
    public string? Title { get; set; }
    public string? AvatarUrl { get; set; }
    public Guid? WorkspaceID { get; set; }
    public virtual Workspace? Workspace { get; set; }
    [MaxLength(200)]
    public string? LastMessagePreview { get; set; } // Nội dung xem trước ("Đã gửi một ảnh", "Hello world"...)
    public DateTime? LastMessageTimestamp { get; set; } // Thời gian của tin nhắn cuối
    [MaxLength(100)]
    public string? LastMessageSenderName { get; set; } // Tên người gửi tin nhắn cuối
    public virtual ICollection<ConversationParticipants> Participants { get; set; } = new HashSet<ConversationParticipants>();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
