using FastBiteGroup.Application.Constants;
using FastBiteGroup.Application.UseCases.V1.Workspace;
using FastBiteGroup.Contract.Services.V1.Workspace;
using FastBiteGroup.Contract.Services.V1.Workspace.Commands;
using FastBiteGroup.Contract.Services.V1.Workspace.Responses;
using FastBiteGroup.Domain.Abstractions;
using System.Security.Cryptography;
using FastBiteGroup.Domain.Enums;

namespace FastBiteGroup.Application.UseCases.V1.Commands.Workspace;

public sealed class UpdateWorkspaceCommandHandler(
    IWorkspaceRepository workspaceRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    ICacheService cacheService)
    : ICommandHandler<UpdateWorkspaceCommand, WorkspaceResponse>
{
    public async Task<Result<WorkspaceResponse>> Handle(UpdateWorkspaceCommand request, CancellationToken cancellationToken)
    {
        var member = await workspaceRepository.GetActiveMemberAsync(request.WorkspaceId, currentUser.UserId, cancellationToken);
        if (member is null)
            return Result.Failure<WorkspaceResponse>(WorkspaceErrors.Forbidden);
        if (member.Role is not (EnumWorkspaceRole.Owner or EnumWorkspaceRole.Admin))
            return Result.Failure<WorkspaceResponse>(WorkspaceErrors.Forbidden);

        var workspace = await workspaceRepository.GetWorkspaceForUpdateAsync(request.WorkspaceId, cancellationToken);
        if (workspace is null || workspace.IsArchived)
            return Result.Failure<WorkspaceResponse>(WorkspaceErrors.NotFound);

        workspace.WorkspaceName = request.WorkspaceName.Trim();
        workspace.Description = request.Description?.Trim();
        workspace.IsChatEnabled = request.IsChatEnabled;
        workspace.IsFeedEnabled = request.IsFeedEnabled;
        workspace.Privacy = (EnumWorkspacePrivacy)request.Privacy;
        workspace.WorkspaceAvatarUrl = request.WorkspaceAvatarUrl?.Trim() ?? string.Empty;
        workspace.UpdatedByUserID = currentUser.UserId;
        workspace.UpdatedAt = DateTimeOffset.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var workspaceMember in workspace.Members.Where(m => m.Status == EnumWorkspaceMemberStatus.Active && m.LeftAt == null))
            await cacheService.RemoveAsync(CacheKeys.UserWorkspaces(workspaceMember.UserID), cancellationToken);

        var summary = await workspaceRepository.GetWorkspaceSummaryForMemberAsync(request.WorkspaceId, currentUser.UserId, cancellationToken);
        return summary is null
            ? Result.Failure<WorkspaceResponse>(WorkspaceErrors.NotFound)
            : Result.Success(summary.ToResponse());
    }
}

public sealed class AcceptWorkspaceInvitationCommandHandler(
    IWorkspaceRepository workspaceRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    ICacheService cacheService)
    : ICommandHandler<AcceptWorkspaceInvitationCommand, WorkspaceResponse>
{
    public async Task<Result<WorkspaceResponse>> Handle(AcceptWorkspaceInvitationCommand request, CancellationToken cancellationToken)
    {
        var email = WorkspaceEmail.Normalize(currentUser.Email);
        var invitation = await workspaceRepository.GetPendingUserInvitationForUpdateAsync(request.InvitationId, email, cancellationToken);
        if (invitation is null || invitation.Workspace is null)
            return Result.Failure<WorkspaceResponse>(WorkspaceErrors.InvitationNotFound);
        if (invitation.Workspace.IsArchived)
            return Result.Failure<WorkspaceResponse>(WorkspaceErrors.NotFound);
        if (invitation.ExpiresAt.HasValue && invitation.ExpiresAt <= DateTimeOffset.UtcNow)
            return Result.Failure<WorkspaceResponse>(WorkspaceErrors.InvitationExpired);

        var member = await workspaceRepository.GetMemberForUpdateAsync(invitation.WorkspaceID, currentUser.UserId, cancellationToken);
        if (member is { Status: EnumWorkspaceMemberStatus.Active, LeftAt: null })
            return Result.Failure<WorkspaceResponse>(WorkspaceErrors.AlreadyMember);
        if (member is { Status: EnumWorkspaceMemberStatus.Banned })
            return Result.Failure<WorkspaceResponse>(WorkspaceErrors.Forbidden);

        if (member is null)
        {
            workspaceRepository.AddMember(new WorkspaceMember
            {
                WorkspaceID = invitation.WorkspaceID,
                UserID = currentUser.UserId,
                Role = EnumWorkspaceRole.Member,
                Status = EnumWorkspaceMemberStatus.Active,
                JoinedAt = DateTime.UtcNow
            });
        }
        else
        {
            member.Role = EnumWorkspaceRole.Member;
            member.Status = EnumWorkspaceMemberStatus.Active;
            member.LeftAt = null;
            member.JoinedAt = DateTime.UtcNow;
        }

        invitation.InvitedUserID = currentUser.UserId;
        invitation.Status = EnumInvitationStatus.Accepted;
        invitation.RespondedAt = DateTimeOffset.UtcNow;
        invitation.UpdatedAt = DateTimeOffset.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await cacheService.RemoveAsync(CacheKeys.UserWorkspaces(currentUser.UserId), cancellationToken);

        var summary = await workspaceRepository.GetWorkspaceSummaryForMemberAsync(invitation.WorkspaceID, currentUser.UserId, cancellationToken);
        return summary is null
            ? Result.Failure<WorkspaceResponse>(WorkspaceErrors.NotFound)
            : Result.Success(summary.ToResponse());
    }
}

public sealed class JoinWorkspaceCommandHandler(
    IWorkspaceRepository workspaceRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    ICacheService cacheService)
    : ICommandHandler<JoinWorkspaceCommand, WorkspaceResponse>
{
    public async Task<Result<WorkspaceResponse>> Handle(JoinWorkspaceCommand request, CancellationToken cancellationToken)
    {
        var invitationCode = request.InvitationCode.Trim().ToUpperInvariant();
        var invitation = await workspaceRepository.GetWorkspaceInvitationByCodeForUpdateAsync(
            invitationCode,
            cancellationToken);

        if (invitation is null || invitation.Workspace is null)
            return Result.Failure<WorkspaceResponse>(WorkspaceErrors.InvitationNotFound);
        if (invitation.Workspace.IsArchived)
            return Result.Failure<WorkspaceResponse>(WorkspaceErrors.NotFound);
        if (!invitation.IsActive)
            return Result.Failure<WorkspaceResponse>(WorkspaceErrors.InvitationInactive);
        if (invitation.ExpiresAt.HasValue && invitation.ExpiresAt <= DateTime.UtcNow)
            return Result.Failure<WorkspaceResponse>(WorkspaceErrors.InvitationExpired);
        if (invitation.MaxUses.HasValue && invitation.CurrentUses >= invitation.MaxUses)
            return Result.Failure<WorkspaceResponse>(WorkspaceErrors.InvitationMaxUsesReached);

        var member = await workspaceRepository.GetMemberForUpdateAsync(invitation.WorkspaceID, currentUser.UserId, cancellationToken);
        if (member is { Status: EnumWorkspaceMemberStatus.Active, LeftAt: null })
            return Result.Failure<WorkspaceResponse>(WorkspaceErrors.AlreadyMember);
        if (member is { Status: EnumWorkspaceMemberStatus.Banned })
            return Result.Failure<WorkspaceResponse>(WorkspaceErrors.Forbidden);

        if (member is null)
        {
            workspaceRepository.AddMember(new WorkspaceMember
            {
                WorkspaceID = invitation.WorkspaceID,
                UserID = currentUser.UserId,
                Role = EnumWorkspaceRole.Member,
                Status = EnumWorkspaceMemberStatus.Active,
                JoinedAt = DateTime.UtcNow
            });
        }
        else
        {
            member.Role = EnumWorkspaceRole.Member;
            member.Status = EnumWorkspaceMemberStatus.Active;
            member.LeftAt = null;
            member.JoinedAt = DateTime.UtcNow;
        }

        invitation.CurrentUses++;
        invitation.UpdatedAt = DateTimeOffset.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await cacheService.RemoveAsync(CacheKeys.UserWorkspaces(currentUser.UserId), cancellationToken);

        var summary = await workspaceRepository.GetWorkspaceSummaryForMemberAsync(invitation.WorkspaceID, currentUser.UserId, cancellationToken);
        return summary is null
            ? Result.Failure<WorkspaceResponse>(WorkspaceErrors.NotFound)
            : Result.Success(summary.ToResponse());
    }
}

public sealed class CreateWorkspaceInviteLinkCommandHandler(
    IWorkspaceRepository workspaceRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : ICommandHandler<CreateWorkspaceInviteLinkCommand, WorkspaceInviteLinkResponse>
{
    public async Task<Result<WorkspaceInviteLinkResponse>> Handle(CreateWorkspaceInviteLinkCommand request, CancellationToken cancellationToken)
    {
        var requester = await workspaceRepository.GetActiveMemberAsync(request.WorkspaceId, currentUser.UserId, cancellationToken);
        if (requester is null || requester.Role is not (EnumWorkspaceRole.Owner or EnumWorkspaceRole.Admin))
            return Result.Failure<WorkspaceInviteLinkResponse>(WorkspaceErrors.Forbidden);

        var workspace = await workspaceRepository.GetWorkspaceForUpdateAsync(request.WorkspaceId, cancellationToken);
        if (workspace is null || workspace.IsArchived)
            return Result.Failure<WorkspaceInviteLinkResponse>(WorkspaceErrors.NotFound);

        var code = await GenerateUniqueInvitationCodeAsync(cancellationToken);
        var invitation = new WorkspaceInvitation
        {
            WorkspaceID = request.WorkspaceId,
            InvitationCode = code,
            CreatedByUserID = currentUser.UserId,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = request.ExpiresAt?.UtcDateTime,
            MaxUses = request.MaxUses,
            CurrentUses = 0,
            IsActive = true
        };

        workspaceRepository.AddWorkspaceInvitation(invitation);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(invitation.ToResponse());
    }

    private async Task<string> GenerateUniqueInvitationCodeAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var bytes = RandomNumberGenerator.GetBytes(8);
            var code = Convert.ToHexString(bytes);

            if (!await workspaceRepository.InvitationCodeExistsAsync(code, cancellationToken))
                return code;
        }

        return Guid.NewGuid().ToString("N")[..16].ToUpperInvariant();
    }
}

public sealed class ArchiveWorkspaceCommandHandler(
    IWorkspaceRepository workspaceRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    ICacheService cacheService)
    : ICommandHandler<ArchiveWorkspaceCommand>
{
    public async Task<Result> Handle(ArchiveWorkspaceCommand request, CancellationToken cancellationToken)
    {
        var member = await workspaceRepository.GetActiveMemberAsync(request.WorkspaceId, currentUser.UserId, cancellationToken);
        if (member is null || member.Role != EnumWorkspaceRole.Owner)
            return Result.Failure(WorkspaceErrors.Forbidden);

        var workspace = await workspaceRepository.GetWorkspaceForUpdateAsync(request.WorkspaceId, cancellationToken);
        if (workspace is null || workspace.IsArchived)
            return Result.Failure(WorkspaceErrors.NotFound);

        workspace.IsArchived = true;
        workspace.UpdatedByUserID = currentUser.UserId;
        workspace.UpdatedAt = DateTimeOffset.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var workspaceMember in workspace.Members.Where(m => m.Status == EnumWorkspaceMemberStatus.Active))
            await cacheService.RemoveAsync(CacheKeys.UserWorkspaces(workspaceMember.UserID), cancellationToken);

        return Result.Success();
    }
}
