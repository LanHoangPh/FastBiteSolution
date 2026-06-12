using FastBiteGroup.Domain.Entities;
using FastBiteGroup.Domain.Enums;

namespace FastBiteGroup.Domain.Abstractions.Repositories;

public interface IWorkspaceRepository : IRepositoryBase<Workspace, Guid>
{
    Task<List<WorkspaceSummary>> GetActiveWorkspacesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<WorkspaceSummary?> GetWorkspaceSummaryForMemberAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken = default);
    Task<Workspace?> GetWorkspaceForUpdateAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    Task<WorkspaceMember?> GetMemberForUpdateAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken = default);
    Task<WorkspaceMember?> GetActiveMemberAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken = default);
    Task<List<WorkspaceMemberSummary>> GetActiveMembersAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    Task<int> CountActiveOwnersAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    Task<bool> HasPendingInvitationAsync(Guid workspaceId, string invitedEmail, CancellationToken cancellationToken = default);
    Task<List<WorkspaceInvitationSummary>> GetPendingInvitationsByEmailAsync(string invitedEmail, CancellationToken cancellationToken = default);
    Task<UserWorkspaceInvitation?> GetPendingUserInvitationForUpdateAsync(int invitationId, string invitedEmail, CancellationToken cancellationToken = default);
    Task<WorkspaceInvitation?> GetWorkspaceInvitationByCodeForUpdateAsync(string invitationCode, CancellationToken cancellationToken = default);
    Task<bool> InvitationCodeExistsAsync(string invitationCode, CancellationToken cancellationToken = default);
    void AddMember(WorkspaceMember member);
    void AddUserInvitation(UserWorkspaceInvitation invitation);
    void AddWorkspaceInvitation(WorkspaceInvitation invitation);
}

public sealed record WorkspaceSummary(
    Guid WorkspaceId,
    string WorkspaceName,
    string? Description,
    bool IsChatEnabled,
    bool IsFeedEnabled,
    EnumWorkspacePrivacy Privacy,
    string WorkspaceAvatarUrl,
    EnumWorkspaceRole CurrentUserRole,
    DateTimeOffset CreatedAt,
    bool IsArchived,
    int MemberCount);

public sealed record WorkspaceMemberSummary(
    int WorkspaceMemberId,
    Guid UserId,
    string Email,
    string FullName,
    string? AvatarUrl,
    EnumWorkspaceRole Role,
    EnumWorkspaceMemberStatus Status,
    DateTime JoinedAt);

public sealed record WorkspaceInvitationSummary(
    int InvitationId,
    Guid WorkspaceId,
    string WorkspaceName,
    string? Description,
    string WorkspaceAvatarUrl,
    Guid InvitedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ExpiresAt);
