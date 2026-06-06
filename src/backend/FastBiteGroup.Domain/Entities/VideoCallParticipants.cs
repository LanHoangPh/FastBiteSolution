namespace FastBiteGroup.Domain.Entities;

public class VideoCallParticipants
{
    public int VideoCallParticipantID { get; set; }
    public Guid VideoCallSessionID { get; set; }
    public Guid UserID { get; set; }
    public DateTime JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
    public virtual VideoCallSessions? VideoCallSession { get; set; }
}

