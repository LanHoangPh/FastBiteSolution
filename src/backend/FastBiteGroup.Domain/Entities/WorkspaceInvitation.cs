using System.ComponentModel.DataAnnotations;
using FastBiteGroup.Domain.Abstractions;

namespace FastBiteGroup.Domain.Entities;

public class WorkspaceInvitation : EntityBase<int>, IDateTracking
{
    public string InvitationCode { get; set; } = string.Empty;
    public Guid WorkspaceID { get; set; }
    public Guid CreatedByUserID { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int? MaxUses { get; set; }
    public int CurrentUses { get; set; }
    public bool IsActive { get; set; } = true;
    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public virtual Workspace? Workspace { get; set; }
}
