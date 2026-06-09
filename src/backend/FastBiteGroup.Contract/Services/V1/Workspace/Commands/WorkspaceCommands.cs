using FastBiteGroup.Contract.Abstractions.Message;
using FastBiteGroup.Contract.Services.V1.Workspace.Responses;

namespace FastBiteGroup.Contract.Services.V1.Workspace.Commands;

public sealed record UpdateWorkspaceCommand(
    Guid WorkspaceId,
    string WorkspaceName,
    string? Description,
    bool IsChatEnabled,
    bool IsFeedEnabled,
    int Privacy,
    string? WorkspaceAvatarUrl) : ICommand<WorkspaceResponse>;

public sealed record InviteWorkspaceMemberCommand(
    Guid WorkspaceId,
    string Email) : ICommand<WorkspaceInvitationResponse>;

public sealed record AcceptWorkspaceInvitationCommand(
    int InvitationId) : ICommand<WorkspaceResponse>;

public sealed record JoinWorkspaceCommand(
    string InvitationCode) : ICommand<WorkspaceResponse>;

public sealed record CreateWorkspaceInviteLinkCommand(
    Guid WorkspaceId,
    DateTimeOffset? ExpiresAt,
    int? MaxUses) : ICommand<WorkspaceInviteLinkResponse>;

public sealed record ArchiveWorkspaceCommand(
    Guid WorkspaceId) : ICommand;
