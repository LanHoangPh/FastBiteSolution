using FastBiteGroup.Domain.Abstractions;

namespace FastBiteGroup.Domain.Entities;

public class ContentReport : EntityAuditBase<int>
{
    public int ReportedContentID { get; set; }
    public EnumReportedContentType ReportedContentType { get; set; }
    public Guid ReportedByUserID { get; set; }
    /// <summary>
    /// ID của người dùng sở hữu nội dung bị báo cáo.
    /// </summary>
    public Guid ReportedContentOwnerId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public EnumContentReportStatus Status { get; set; } = EnumContentReportStatus.Pending;
    public Guid GroupID { get; set; }
    public Group Group { get; set; } = null!;
}

