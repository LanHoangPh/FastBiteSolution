using FastBiteGroup.Domain.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace FastBiteGroup.Domain.Entities;

public class Workspace : EntityBase<Guid>, IDateTracking, ISoftDelete
{
    
    public string WorkspaceName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public EnumWorkspaceType WorkspaceType { get; set; } = EnumWorkspaceType.Public;
    public EnumWorkspacePrivacy Privacy { get; set; } = EnumWorkspacePrivacy.Public;
    public string WorkspaceAvatarUrl { get; set; } = string.Empty;
    public bool IsArchived { get; set; } // trạng thái nhóm có bị lưu trữ hay không
    public Guid CreatedByUserID { get; set; }
    public Guid? UpdatedByUserID { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    //[Timestamp]
    //public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    public int? ConversationId { get; set; }
    public virtual Conversation? Conversation { get; set; }

    public virtual ICollection<WorkspaceMember> Members { get; set; } = new HashSet<WorkspaceMember>();
    public virtual ICollection<Posts> Posts { get; set; } = new HashSet<Posts>();
    public virtual ICollection<WorkspaceInvitation> WorkspaceInvitations { get; set; } = new HashSet<WorkspaceInvitation>();
    public virtual ICollection<UserWorkspaceInvitation> DirectUserInvitations { get; set; } = new HashSet<UserWorkspaceInvitation>();
    public virtual ICollection<ContentReport> ContentReports { get; set; } = new HashSet<ContentReport>();
}
