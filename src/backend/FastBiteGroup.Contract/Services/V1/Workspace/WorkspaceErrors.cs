using FastBiteGroup.Contract.Abstractions.Shared;

namespace FastBiteGroup.Contract.Services.V1.Workspace;

public static class WorkspaceErrors
{
    public static readonly Error NotFound = new(
        "Workspace.NotFound",
        "Workspace was not found.");

    public static readonly Error Forbidden = new(
        "Workspace.Forbidden",
        "You do not have permission to access this workspace.");

    public static readonly Error InvitationNotFound = new(
        "Workspace.InvitationNotFound",
        "Workspace invitation was not found.");

    public static readonly Error InvitationExpired = new(
        "Workspace.InvitationExpired",
        "Workspace invitation has expired.");

    public static readonly Error InvitationInactive = new(
        "Workspace.InvitationInactive",
        "Workspace invitation is not active.");

    public static readonly Error InvitationMaxUsesReached = new(
        "Workspace.InvitationMaxUsesReached",
        "Workspace invitation has reached its usage limit.");

    public static readonly Error InvitationAlreadyExists = new(
        "Workspace.InvitationConflict",
        "A pending invitation already exists for this email in the workspace.");

    public static readonly Error AlreadyMember = new(
        "Workspace.MemberConflict",
        "User is already an active member of this workspace.");

    public static readonly Error LastOwnerRequired = new(
        "Workspace.LastOwnerRequired",
        "Workspace must keep at least one active owner.");
}
