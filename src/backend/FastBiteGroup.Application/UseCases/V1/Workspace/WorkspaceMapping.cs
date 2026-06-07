using FastBiteGroup.Contract.Services.V1.Workspace.Responses;
using FastBiteGroup.Domain.Abstractions.Repositories;
using FastBiteGroup.Domain.Entities;

namespace FastBiteGroup.Application.UseCases.V1.Workspace;

internal static class WorkspaceMapping
{
    public static WorkspaceResponse ToResponse(this WorkspaceSummary summary)
    {
        return new WorkspaceResponse(
            summary.WorkspaceId,
            summary.WorkspaceName,
            summary.Description,
            summary.WorkspaceType.ToString(),
            summary.Privacy.ToString(),
            summary.WorkspaceAvatarUrl,
            summary.CurrentUserRole.ToString(),
            summary.CreatedAt,
            summary.IsArchived,
            summary.MemberCount);
    }

    public static WorkspaceDetailResponse ToDetailResponse(this WorkspaceSummary summary)
    {
        return new WorkspaceDetailResponse(
            summary.WorkspaceId,
            summary.WorkspaceName,
            summary.Description,
            summary.WorkspaceType.ToString(),
            summary.Privacy.ToString(),
            summary.WorkspaceAvatarUrl,
            summary.CurrentUserRole.ToString(),
            summary.CreatedAt,
            summary.IsArchived,
            summary.MemberCount);
    }

    public static WorkspaceMemberResponse ToResponse(this WorkspaceMemberSummary summary)
    {
        return new WorkspaceMemberResponse(
            summary.WorkspaceMemberId,
            summary.UserId,
            summary.Email,
            summary.FullName,
            summary.AvatarUrl,
            summary.Role.ToString(),
            summary.Status.ToString(),
            summary.JoinedAt);
    }

    public static WorkspaceInvitationResponse ToResponse(this WorkspaceInvitationSummary summary)
    {
        return new WorkspaceInvitationResponse(
            summary.InvitationId,
            summary.WorkspaceId,
            summary.WorkspaceName,
            summary.Description,
            summary.WorkspaceAvatarUrl,
            summary.InvitedByUserId,
            summary.CreatedAt,
            summary.ExpiresAt);
    }

    public static WorkspaceInviteLinkResponse ToResponse(this WorkspaceInvitation invitation)
    {
        return new WorkspaceInviteLinkResponse(
            invitation.InvitationID,
            invitation.WorkspaceID,
            invitation.InvitationCode,
            invitation.CreatedAt,
            invitation.ExpiresAt,
            invitation.MaxUses,
            invitation.CurrentUses,
            invitation.IsActive);
    }
}
