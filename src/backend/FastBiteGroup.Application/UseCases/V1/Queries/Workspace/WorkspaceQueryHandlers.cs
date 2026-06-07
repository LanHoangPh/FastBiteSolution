using FastBiteGroup.Application.Abstractions.Authentication;
using FastBiteGroup.Application.UseCases.V1.Workspace;
using FastBiteGroup.Contract.Abstractions.Message;
using FastBiteGroup.Contract.Abstractions.Shared;
using FastBiteGroup.Contract.Services.V1.Workspace;
using FastBiteGroup.Contract.Services.V1.Workspace.Queries;
using FastBiteGroup.Contract.Services.V1.Workspace.Responses;
using FastBiteGroup.Domain.Abstractions.Repositories;

namespace FastBiteGroup.Application.UseCases.V1.Queries.Workspace;

public sealed class GetWorkspaceByIdQueryHandler : IQueryHandler<GetWorkspaceByIdQuery, WorkspaceDetailResponse>
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly ICurrentUser _currentUser;

    public GetWorkspaceByIdQueryHandler(IWorkspaceRepository workspaceRepository, ICurrentUser currentUser)
    {
        _workspaceRepository = workspaceRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<WorkspaceDetailResponse>> Handle(GetWorkspaceByIdQuery request, CancellationToken cancellationToken)
    {
        var summary = await _workspaceRepository.GetWorkspaceSummaryForMemberAsync(
            request.WorkspaceId,
            _currentUser.UserId,
            cancellationToken);

        return summary is null
            ? Result.Failure<WorkspaceDetailResponse>(WorkspaceErrors.Forbidden)
            : Result.Success(summary.ToDetailResponse());
    }
}

public sealed class GetWorkspaceMembersQueryHandler : IQueryHandler<GetWorkspaceMembersQuery, List<WorkspaceMemberResponse>>
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly ICurrentUser _currentUser;

    public GetWorkspaceMembersQueryHandler(IWorkspaceRepository workspaceRepository, ICurrentUser currentUser)
    {
        _workspaceRepository = workspaceRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<List<WorkspaceMemberResponse>>> Handle(GetWorkspaceMembersQuery request, CancellationToken cancellationToken)
    {
        var member = await _workspaceRepository.GetActiveMemberAsync(request.WorkspaceId, _currentUser.UserId, cancellationToken);
        if (member is null)
            return Result.Failure<List<WorkspaceMemberResponse>>(WorkspaceErrors.Forbidden);

        var members = await _workspaceRepository.GetActiveMembersAsync(request.WorkspaceId, cancellationToken);
        return Result.Success(members.Select(m => m.ToResponse()).ToList());
    }
}

public sealed class GetMyWorkspaceInvitationsQueryHandler : IQueryHandler<GetMyWorkspaceInvitationsQuery, List<WorkspaceInvitationResponse>>
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly ICurrentUser _currentUser;

    public GetMyWorkspaceInvitationsQueryHandler(IWorkspaceRepository workspaceRepository, ICurrentUser currentUser)
    {
        _workspaceRepository = workspaceRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<List<WorkspaceInvitationResponse>>> Handle(GetMyWorkspaceInvitationsQuery request, CancellationToken cancellationToken)
    {
        var invitations = await _workspaceRepository.GetPendingInvitationsByEmailAsync(
            WorkspaceEmail.Normalize(_currentUser.Email),
            cancellationToken);

        return Result.Success(invitations.Select(i => i.ToResponse()).ToList());
    }
}
