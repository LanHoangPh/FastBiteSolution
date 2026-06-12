using FastBiteGroup.Domain.Abstractions;
using FastBiteGroup.Domain.Enums;

namespace FastBiteGroup.Domain.Entities;

public class UserWorkspaceInvitation : EntityBase<int>, IDateTracking
{
    public Guid WorkspaceID { get; set; }
    public virtual Workspace? Workspace { get; set; }
    public Guid? InvitedUserID { get; set; }
    public string InvitedEmail { get; set; } = string.Empty;
    public Guid InvitedByUserID { get; set; }
    public EnumInvitationStatus Status { get; set; } = EnumInvitationStatus.Pending;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? RespondedAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
}
