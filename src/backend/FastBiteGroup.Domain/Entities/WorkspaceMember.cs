using FastBiteGroup.Domain.Abstractions;
using FastBiteGroup.Domain.Enums;

namespace FastBiteGroup.Domain.Entities;

public class WorkspaceMember : EntityBase<int>
{
    public EnumWorkspaceRole Role { get; set; } = EnumWorkspaceRole.Member;
    public EnumWorkspaceMemberStatus Status { get; set; } = EnumWorkspaceMemberStatus.Active;
    public DateTime JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
    public Guid WorkspaceID { get; set; }
    public virtual Workspace? Workspace { get; set; }
    public Guid UserID { get; set; }
}
