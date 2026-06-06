using System.ComponentModel.DataAnnotations;

namespace FastBiteGroup.Domain.Entities;

public class GroupInvitations : IDateTracking
{
    public int InvitationID { get; set; }
    public string InvitationCode { get; set; } = string.Empty;
    public Guid GroupID { get; set; }
    public Guid CreatedByUserID { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int? MaxUses { get; set; }
    public int CurrentUses { get; set; }
    public bool IsActive { get; set; } = true;
    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public virtual Group? Group { get; set; }
}

