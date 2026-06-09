using FastBiteGroup.Contract.Abstractions.Message;
using FastBiteGroup.Contract.Services.V1.Workspace.Responses;

namespace FastBiteGroup.Contract.Services.V1.Workspace.Commands;

public record CreateWorkspaceCommand(
    string WorkspaceName,
    string? Description,
    bool IsChatEnabled,
    bool IsFeedEnabled,
    int Privacy,
    string? WorkspaceAvatarUrl
) : ICommand<WorkspaceResponse>;
