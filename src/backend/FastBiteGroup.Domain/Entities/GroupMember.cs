namespace FastBiteGroup.Domain.Entities;

public class GroupMember
{
    public int GroupMemberID { get; set; }
    public EnumGroupRole Role { get; set; } = EnumGroupRole.Member;
    public DateTime JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
    public Guid GroupID { get; set; }
    public virtual Group? Group { get; set; }
    public Guid UserID { get; set; }
}

