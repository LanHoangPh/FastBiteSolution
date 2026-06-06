namespace FastBiteGroup.Domain.Entities;

public class PollOptions
{
    public int PollOptionID { get; set; }
    public int PollID { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public virtual Polls? Poll { get; set; }
    public virtual ICollection<PollVotes> Votes { get; set; } = new HashSet<PollVotes>();
}

