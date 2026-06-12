using FastBiteGroup.Application.UseCases.V1.Workspace;
using FastBiteGroup.Contract.Services.V1.Workspace;
using FastBiteGroup.Contract.Services.V1.Workspace.Commands;
using FastBiteGroup.Contract.Services.V1.Workspace.Responses;
using FastBiteGroup.Domain.Abstractions;
using FastBiteGroup.Domain.Enums;

namespace FastBiteGroup.Application.UseCases.V1.Commands.Workspace;

public sealed class InviteWorkspaceMemberCommandHandler(
    IWorkspaceRepository workspaceRepository,
    IUserAuthService userAuthService,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : ICommandHandler<InviteWorkspaceMemberCommand, WorkspaceInvitationResponse>
{
    public async Task<Result<WorkspaceInvitationResponse>> Handle(InviteWorkspaceMemberCommand request, CancellationToken cancellationToken)
    {
        var requester = await workspaceRepository.GetActiveMemberAsync(request.WorkspaceId, currentUser.UserId, cancellationToken);
        if (requester is null || requester.Role is not (EnumWorkspaceRole.Owner or EnumWorkspaceRole.Admin))
            return Result.Failure<WorkspaceInvitationResponse>(WorkspaceErrors.Forbidden);

        var workspace = await workspaceRepository.GetWorkspaceForUpdateAsync(request.WorkspaceId, cancellationToken);
        if (workspace is null || workspace.IsArchived)
            return Result.Failure<WorkspaceInvitationResponse>(WorkspaceErrors.NotFound);

        var invitedEmail = WorkspaceEmail.Normalize(request.Email);
        if (await workspaceRepository.HasPendingInvitationAsync(request.WorkspaceId, invitedEmail, cancellationToken))
            return Result.Failure<WorkspaceInvitationResponse>(WorkspaceErrors.InvitationAlreadyExists);

        var invitedUser = await userAuthService.FindByEmailAsync(invitedEmail, cancellationToken);
        if (invitedUser is not null)
        {
            var existingMember = await workspaceRepository.GetActiveMemberAsync(request.WorkspaceId, invitedUser.Id, cancellationToken);
            if (existingMember is not null)
                return Result.Failure<WorkspaceInvitationResponse>(WorkspaceErrors.AlreadyMember);
        }

        var invitation = new UserWorkspaceInvitation
        {
            WorkspaceID = request.WorkspaceId,
            InvitedEmail = invitedEmail,
            InvitedUserID = invitedUser?.Id,
            InvitedByUserID = currentUser.UserId,
            Status = EnumInvitationStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7)
        };

        workspaceRepository.AddUserInvitation(invitation);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new WorkspaceInvitationResponse(
            invitation.Id,
            workspace.Id,
            workspace.WorkspaceName,
            workspace.Description,
            workspace.WorkspaceAvatarUrl,
            invitation.InvitedByUserID,
            invitation.CreatedAt,
            invitation.ExpiresAt);

        return Result.Success(response);
    }
}
