using System.ComponentModel.DataAnnotations;

namespace FastBiteGroup.Domain.Entities;

public class AdminNotifications
{
    public long Id { get; set; } 

    public EnumAdminNotificationType NotificationType { get; set; }

    [MaxLength(500)]
    public string Message { get; set; } = string.Empty;

    // Link để điều hướng khi Admin click vào, ví dụ: "/admin/reports/posts/123"
    public string? LinkTo { get; set; }

    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // (Tùy chọn) Ghi lại ai đã gây ra sự kiện này
    public Guid? TriggeredByUserId { get; set; }
}

