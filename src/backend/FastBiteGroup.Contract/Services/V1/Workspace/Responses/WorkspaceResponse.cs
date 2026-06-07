namespace FastBiteGroup.Contract.Services.V1.Workspace.Responses;

public record WorkspaceResponse(
    Guid WorkspaceId,
    string WorkspaceName,
    string? Description,
    string WorkspaceType,
    string Privacy,
    string WorkspaceAvatarUrl,
    string CurrentUserRole,
    DateTimeOffset CreatedAt,
    bool IsArchived,
    int MemberCount
);

public sealed record WorkspaceDetailResponse(
    Guid WorkspaceId,
    string WorkspaceName,
    string? Description,
    string WorkspaceType,
    string Privacy,
    string WorkspaceAvatarUrl,
    string CurrentUserRole,
    DateTimeOffset CreatedAt,
    bool IsArchived,
    int MemberCount);

public sealed record WorkspaceMemberResponse(
    int WorkspaceMemberId,
    Guid UserId,
    string Email,
    string FullName,
    string? AvatarUrl,
    string Role,
    string Status,
    DateTime JoinedAt);

public sealed record WorkspaceInvitationResponse(
    int InvitationId,
    Guid WorkspaceId,
    string WorkspaceName,
    string? Description,
    string WorkspaceAvatarUrl,
    Guid InvitedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ExpiresAt);

public sealed record WorkspaceInviteLinkResponse(
    int InvitationId,
    Guid WorkspaceId,
    string InvitationCode,
    DateTimeOffset CreatedAt,
    DateTime? ExpiresAt,
    int? MaxUses,
    int CurrentUses,
    bool IsActive);
