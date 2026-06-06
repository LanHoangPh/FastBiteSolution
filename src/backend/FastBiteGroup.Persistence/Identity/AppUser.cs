using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace FastBiteGroup.Persistence.Identity;

public class AppUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string? FullName { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public bool IsActive { get; set; } = true;
    public EnumUserPresenceStatus PresenceStatus { get; set; } = EnumUserPresenceStatus.Offline;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public EnumMessagingPrivacy MessagingPrivacy { get; set; } = EnumMessagingPrivacy.FromSharedGroupMembers;
    public string? OneSignalPlayerId { get; set; }
    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}

