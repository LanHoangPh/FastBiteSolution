using FastBiteGroup.Domain.Enum;

namespace FastBiteGroup.Domain.Entities;

public class WorkspaceMember
{
    public int WorkspaceMemberID { get; set; }
    public EnumWorkspaceRole Role { get; set; } = EnumWorkspaceRole.Member;
    public DateTime JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
    public Guid WorkspaceID { get; set; }
    public virtual Workspace? Workspace { get; set; }
    public Guid UserID { get; set; }
}
