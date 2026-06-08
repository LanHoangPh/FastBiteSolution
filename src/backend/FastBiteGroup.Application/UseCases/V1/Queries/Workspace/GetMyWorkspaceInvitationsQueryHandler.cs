using FastBiteGroup.Application.Abstractions.Authentication;
using FastBiteGroup.Application.UseCases.V1.Workspace;
using FastBiteGroup.Contract.Abstractions.Message;
using FastBiteGroup.Contract.Abstractions.Shared;
using FastBiteGroup.Contract.Services.V1.Workspace.Queries;
using FastBiteGroup.Contract.Services.V1.Workspace.Responses;
using FastBiteGroup.Domain.Abstractions.Repositories;

namespace FastBiteGroup.Application.UseCases.V1.Queries.Workspace;

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
