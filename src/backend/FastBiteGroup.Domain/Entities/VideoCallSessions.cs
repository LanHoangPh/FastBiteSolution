namespace FastBiteGroup.Domain.Entities;

public class VideoCallSessions : ISoftDelete
{
    public Guid VideoCallSessionID { get; set; }
    public string? Title { get; set; }
    public int ConversationID { get; set; }
    public Guid InitiatorUserID { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public EnumCallSessionStatus Status { get; set; } = EnumCallSessionStatus.Ringing;
    /// <summary>
    /// Lưu ID của Hangfire job xử lý timeout.
    /// </summary>
    public string? TimeoutJobId { get; set; }
    public virtual Conversation? Conversation { get; set; }
    public virtual ICollection<VideoCallParticipants> Participants { get; set; } = new HashSet<VideoCallParticipants>();
}

