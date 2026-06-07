using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using FastBiteGroup.Domain.Enums;

namespace FastBiteGroup.Persistence.Identity;

public class AppUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastSeenAt { get; set; }
    public EnumManualStatus ManualStatus { get; set; } = EnumManualStatus.Available;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public EnumMessagingPrivacy MessagingPrivacy { get; set; } = EnumMessagingPrivacy.FromSharedGroupMembers;
    public string? OneSignalPlayerId { get; set; }
    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}

