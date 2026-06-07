using FastBiteGroup.Domain.Enum;

namespace FastBiteGroup.Domain.Entities;

public class UserWorkspaceInvitation : IDateTracking
{
    public int InvitationID { get; set; }

    public Guid WorkspaceID { get; set; }
    public virtual Workspace? Workspace { get; set; }
    public Guid InvitedUserID { get; set; }
    public Guid InvitedByUserID { get; set; }
    public EnumInvitationStatus Status { get; set; } = EnumInvitationStatus.Pending;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? RespondedAt { get; set; }
}

