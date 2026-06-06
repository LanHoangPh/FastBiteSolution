using System.ComponentModel.DataAnnotations;

namespace FastBiteGroup.Domain.Entities;

public class SharedFiles : ISoftDelete
{
    public int FileID { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string StorageUrl { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? FileType { get; set; }
    public Guid UploadedByUserID { get; set; }
    public DateTime UploadedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    /// <summary>
    /// Ngữ cảnh sử dụng của file, ví dụ: "PostAttachment", "UserAvatar", "GroupAvatar".
    /// </summary>
    [StringLength(50)]
    public string FileContext { get; set; } = string.Empty;
}

