using FastBiteGroup.Domain.Abstractions;
using FastBiteGroup.Domain.Enums;

namespace FastBiteGroup.Domain.Entities;

public class ContentReport : EntityAuditBase<int>
{
    public EnumReportedContentType ReportedContentType { get; set; }
    public Guid ReportedByUserID { get; set; }
    /// <summary>
    /// ID của người dùng sở hữu nội dung bị báo cáo.
    /// </summary>
    public Guid ReportedContentOwnerId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public EnumContentReportStatus Status { get; set; } = EnumContentReportStatus.Pending;
    public Guid WorkspaceID { get; set; }
    public Workspace Workspace { get; set; } = null!;
}

