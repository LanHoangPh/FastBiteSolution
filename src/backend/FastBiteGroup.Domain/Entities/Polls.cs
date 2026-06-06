namespace FastBiteGroup.Domain.Entities;

public class Polls : IDateTracking, ISoftDelete
{
    public int PollID { get; set; }
    public int ConversationID { get; set; }
    public Guid CreatedByUserID { get; set; }
    public string Question { get; set; } = string.Empty;
    public bool AllowMultipleChoices { get; set; }
    public DateTime? ClosesAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? MessageID { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public virtual Conversation? Conversation { get; set; }
    public virtual ICollection<PollOptions> Options { get; set; } = new HashSet<PollOptions>();
}

