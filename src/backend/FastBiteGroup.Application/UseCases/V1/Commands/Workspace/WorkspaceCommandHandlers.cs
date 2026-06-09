using FastBiteGroup.Application.Constants;
using FastBiteGroup.Application.UseCases.V1.Workspace;
using FastBiteGroup.Contract.Services.V1.Workspace;
using FastBiteGroup.Contract.Services.V1.Workspace.Commands;
using FastBiteGroup.Contract.Services.V1.Workspace.Responses;
using FastBiteGroup.Domain.Abstractions;
using System.Security.Cryptography;

namespace FastBiteGroup.Application.UseCases.V1.Commands.Workspace;

public sealed class UpdateWorkspaceCommandHandler : ICommandHandler<UpdateWorkspaceCommand, WorkspaceResponse>
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly ICacheService _cacheService;

    public UpdateWorkspaceCommandHandler(
        IWorkspaceRepository workspaceRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        ICacheService cacheService)
    {
        _workspaceRepository = workspaceRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _cacheService = cacheService;
    }

    public async Task<Result<WorkspaceResponse>> Handle(UpdateWorkspaceCommand request, CancellationToken cancellationToken)
    {
        var member = await _workspaceRepository.GetActiveMemberAsync(request.WorkspaceId, _currentUser.UserId, cancellationToken);
        if (member is null)
            return Result.Failure<WorkspaceResponse>(WorkspaceErrors.Forbidden);
        if (member.Role is not (EnumWorkspaceRole.Owner or EnumWorkspaceRole.Admin))
            return Result.Failure<WorkspaceResponse>(WorkspaceErrors.Forbidden);

        var workspace = await _workspaceRepository.GetWorkspaceForUpdateAsync(request.WorkspaceId, cancellationToken);
        if (workspace is null || workspace.IsArchived)
            return Result.Failure<WorkspaceResponse>(WorkspaceErrors.NotFound);

        workspace.WorkspaceName = request.WorkspaceName.Trim();
        workspace.Description = request.Description?.Trim();
        workspace.IsChatEnabled = request.IsChatEnabled;
        workspace.IsFeedEnabled = request.IsFeedEnabled;
        workspace.Privacy = (EnumWorkspacePrivacy)request.Privacy;
        workspace.WorkspaceAvatarUrl = request.WorkspaceAvatarUrl?.Trim() ?? string.Empty;
        workspace.UpdatedByUserID = _currentUser.UserId;
        workspace.UpdatedAt = DateTimeOffset.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var workspaceMember in workspace.Members.Where(m => m.Status == EnumWorkspaceMemberStatus.Active && m.LeftAt == null))
            await _cacheService.RemoveAsync(CacheKeys.UserWorkspaces(workspaceMember.UserID), cancellationToken);

        var summary = await _workspaceRepository.GetWorkspaceSummaryForMemberAsync(request.WorkspaceId, _currentUser.UserId, cancellationToken);
        return summary is null
            ? Result.Failure<WorkspaceResponse>(WorkspaceErrors.NotFound)
            : Result.Success(summary.ToResponse());
    }
}

public sealed class InviteWorkspaceMemberCommandHandler : ICommandHandler<InviteWorkspaceMemberCommand, WorkspaceInvitationResponse>
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IUserAuthService _userAuthService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public InviteWorkspaceMemberCommandHandler(
        IWorkspaceRepository workspaceRepository,
        IUserAuthService userAuthService,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _workspaceRepository = workspaceRepository;
        _userAuthService = userAuthService;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<WorkspaceInvitationResponse>> Handle(InviteWorkspaceMemberCommand request, CancellationToken cancellationToken)
    {
        var requester = await _workspaceRepository.GetActiveMemberAsync(request.WorkspaceId, _currentUser.UserId, cancellationToken);
        if (requester is null || requester.Role is not (EnumWorkspaceRole.Owner or EnumWorkspaceRole.Admin))
            return Result.Failure<WorkspaceInvitationResponse>(WorkspaceErrors.Forbidden);

        var workspace = await _workspaceRepository.GetWorkspaceForUpdateAsync(request.WorkspaceId, cancellationToken);
        if (workspace is null || workspace.IsArchived)
            return Result.Failure<WorkspaceInvitationResponse>(WorkspaceErrors.NotFound);

        var invitedEmail = WorkspaceEmail.Normalize(request.Email);
        if (await _workspaceRepository.HasPendingInvitationAsync(request.WorkspaceId, invitedEmail, cancellationToken))
            return Result.Failure<WorkspaceInvitationResponse>(WorkspaceErrors.InvitationAlreadyExists);

        var invitedUser = await _userAuthService.FindByEmailAsync(invitedEmail, cancellationToken);
        if (invitedUser is not null)
        {
            var existingMember = await _workspaceRepository.GetActiveMemberAsync(request.WorkspaceId, invitedUser.Id, cancellationToken);
            if (existingMember is not null)
                return Result.Failure<WorkspaceInvitationResponse>(WorkspaceErrors.AlreadyMember);
        }

        var invitation = new UserWorkspaceInvitation
        {
            WorkspaceID = request.WorkspaceId,
            InvitedEmail = invitedEmail,
            InvitedUserID = invitedUser?.Id,
            InvitedByUserID = _currentUser.UserId,
            Status = EnumInvitationStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7)
        };

        _workspaceRepository.AddUserInvitation(invitation);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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

public sealed class AcceptWorkspaceInvitationCommandHandler : ICommandHandler<AcceptWorkspaceInvitationCommand, WorkspaceResponse>
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly ICacheService _cacheService;

    public AcceptWorkspaceInvitationCommandHandler(
        IWorkspaceRepository workspaceRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        ICacheService cacheService)
    {
        _workspaceRepository = workspaceRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _cacheService = cacheService;
    }

    public async Task<Result<WorkspaceResponse>> Handle(AcceptWorkspaceInvitationCommand request, CancellationToken cancellationToken)
    {
        var email = WorkspaceEmail.Normalize(_currentUser.Email);
        var invitation = await _workspaceRepository.GetPendingUserInvitationForUpdateAsync(request.InvitationId, email, cancellationToken);
        if (invitation is null || invitation.Workspace is null)
            return Result.Failure<WorkspaceResponse>(WorkspaceErrors.InvitationNotFound);
        if (invitation.Workspace.IsArchived)
            return Result.Failure<WorkspaceResponse>(WorkspaceErrors.NotFound);
        if (invitation.ExpiresAt.HasValue && invitation.ExpiresAt <= DateTimeOffset.UtcNow)
            return Result.Failure<WorkspaceResponse>(WorkspaceErrors.InvitationExpired);

        var member = await _workspaceRepository.GetMemberForUpdateAsync(invitation.WorkspaceID, _currentUser.UserId, cancellationToken);
        if (member is { Status: EnumWorkspaceMemberStatus.Active, LeftAt: null })
            return Result.Failure<WorkspaceResponse>(WorkspaceErrors.AlreadyMember);
        if (member is { Status: EnumWorkspaceMemberStatus.Banned })
            return Result.Failure<WorkspaceResponse>(WorkspaceErrors.Forbidden);

        if (member is null)
        {
            _workspaceRepository.AddMember(new WorkspaceMember
            {
                WorkspaceID = invitation.WorkspaceID,
                UserID = _currentUser.UserId,
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

        invitation.InvitedUserID = _currentUser.UserId;
        invitation.Status = EnumInvitationStatus.Accepted;
        invitation.RespondedAt = DateTimeOffset.UtcNow;
        invitation.UpdatedAt = DateTimeOffset.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cacheService.RemoveAsync(CacheKeys.UserWorkspaces(_currentUser.UserId), cancellationToken);

        var summary = await _workspaceRepository.GetWorkspaceSummaryForMemberAsync(invitation.WorkspaceID, _currentUser.UserId, cancellationToken);
        return summary is null
            ? Result.Failure<WorkspaceResponse>(WorkspaceErrors.NotFound)
            : Result.Success(summary.ToResponse());
    }
}

public sealed class JoinWorkspaceCommandHandler : ICommandHandler<JoinWorkspaceCommand, WorkspaceResponse>
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly ICacheService _cacheService;

    public JoinWorkspaceCommandHandler(
        IWorkspaceRepository workspaceRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        ICacheService cacheService)
    {
        _workspaceRepository = workspaceRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _cacheService = cacheService;
    }

    public async Task<Result<WorkspaceResponse>> Handle(JoinWorkspaceCommand request, CancellationToken cancellationToken)
    {
        var invitationCode = request.InvitationCode.Trim().ToUpperInvariant();
        var invitation = await _workspaceRepository.GetWorkspaceInvitationByCodeForUpdateAsync(
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

        var member = await _workspaceRepository.GetMemberForUpdateAsync(invitation.WorkspaceID, _currentUser.UserId, cancellationToken);
        if (member is { Status: EnumWorkspaceMemberStatus.Active, LeftAt: null })
            return Result.Failure<WorkspaceResponse>(WorkspaceErrors.AlreadyMember);
        if (member is { Status: EnumWorkspaceMemberStatus.Banned })
            return Result.Failure<WorkspaceResponse>(WorkspaceErrors.Forbidden);

        if (member is null)
        {
            _workspaceRepository.AddMember(new WorkspaceMember
            {
                WorkspaceID = invitation.WorkspaceID,
                UserID = _currentUser.UserId,
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

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cacheService.RemoveAsync(CacheKeys.UserWorkspaces(_currentUser.UserId), cancellationToken);

        var summary = await _workspaceRepository.GetWorkspaceSummaryForMemberAsync(invitation.WorkspaceID, _currentUser.UserId, cancellationToken);
        return summary is null
            ? Result.Failure<WorkspaceResponse>(WorkspaceErrors.NotFound)
            : Result.Success(summary.ToResponse());
    }
}

public sealed class CreateWorkspaceInviteLinkCommandHandler : ICommandHandler<CreateWorkspaceInviteLinkCommand, WorkspaceInviteLinkResponse>
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public CreateWorkspaceInviteLinkCommandHandler(
        IWorkspaceRepository workspaceRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _workspaceRepository = workspaceRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<WorkspaceInviteLinkResponse>> Handle(CreateWorkspaceInviteLinkCommand request, CancellationToken cancellationToken)
    {
        var requester = await _workspaceRepository.GetActiveMemberAsync(request.WorkspaceId, _currentUser.UserId, cancellationToken);
        if (requester is null || requester.Role is not (EnumWorkspaceRole.Owner or EnumWorkspaceRole.Admin))
            return Result.Failure<WorkspaceInviteLinkResponse>(WorkspaceErrors.Forbidden);

        var workspace = await _workspaceRepository.GetWorkspaceForUpdateAsync(request.WorkspaceId, cancellationToken);
        if (workspace is null || workspace.IsArchived)
            return Result.Failure<WorkspaceInviteLinkResponse>(WorkspaceErrors.NotFound);

        var code = await GenerateUniqueInvitationCodeAsync(cancellationToken);
        var invitation = new WorkspaceInvitation
        {
            WorkspaceID = request.WorkspaceId,
            InvitationCode = code,
            CreatedByUserID = _currentUser.UserId,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = request.ExpiresAt?.UtcDateTime,
            MaxUses = request.MaxUses,
            CurrentUses = 0,
            IsActive = true
        };

        _workspaceRepository.AddWorkspaceInvitation(invitation);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(invitation.ToResponse());
    }

    private async Task<string> GenerateUniqueInvitationCodeAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var bytes = RandomNumberGenerator.GetBytes(8);
            var code = Convert.ToHexString(bytes);

            if (!await _workspaceRepository.InvitationCodeExistsAsync(code, cancellationToken))
                return code;
        }

        return Guid.NewGuid().ToString("N")[..16].ToUpperInvariant();
    }
}

public sealed class ArchiveWorkspaceCommandHandler : ICommandHandler<ArchiveWorkspaceCommand>
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly ICacheService _cacheService;

    public ArchiveWorkspaceCommandHandler(
        IWorkspaceRepository workspaceRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        ICacheService cacheService)
    {
        _workspaceRepository = workspaceRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _cacheService = cacheService;
    }

    public async Task<Result> Handle(ArchiveWorkspaceCommand request, CancellationToken cancellationToken)
    {
        var member = await _workspaceRepository.GetActiveMemberAsync(request.WorkspaceId, _currentUser.UserId, cancellationToken);
        if (member is null || member.Role != EnumWorkspaceRole.Owner)
            return Result.Failure(WorkspaceErrors.Forbidden);

        var workspace = await _workspaceRepository.GetWorkspaceForUpdateAsync(request.WorkspaceId, cancellationToken);
        if (workspace is null || workspace.IsArchived)
            return Result.Failure(WorkspaceErrors.NotFound);

        workspace.IsArchived = true;
        workspace.UpdatedByUserID = _currentUser.UserId;
        workspace.UpdatedAt = DateTimeOffset.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var workspaceMember in workspace.Members.Where(m => m.Status == EnumWorkspaceMemberStatus.Active))
            await _cacheService.RemoveAsync(CacheKeys.UserWorkspaces(workspaceMember.UserID), cancellationToken);

        return Result.Success();
    }
}
