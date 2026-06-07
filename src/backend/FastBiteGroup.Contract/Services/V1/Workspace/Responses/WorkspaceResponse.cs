namespace FastBiteGroup.Contract.Services.V1.Workspace.Responses;

public record WorkspaceResponse(
    Guid WorkspaceId,
    string WorkspaceName,
    string? Description,
    string WorkspaceType,
    string Privacy,
    string WorkspaceAvatarUrl,
    string CurrentUserRole
);
