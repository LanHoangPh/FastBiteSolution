using FastBiteGroup.Application.UseCases.V1.Workspace;
using FastBiteGroup.Contract.Services.V1.Workspace.Queries;
using FastBiteGroup.Contract.Services.V1.Workspace.Responses;

namespace FastBiteGroup.Application.UseCases.V1.Queries.Workspace;

public sealed class GetMyWorkspaceInvitationsQueryHandler(
    IWorkspaceRepository workspaceRepository,
    ICurrentUser currentUser)
    : IQueryHandler<GetMyWorkspaceInvitationsQuery, List<WorkspaceInvitationResponse>>
{
    public async Task<Result<List<WorkspaceInvitationResponse>>> Handle(GetMyWorkspaceInvitationsQuery request, CancellationToken cancellationToken)
    {
        var invitations = await workspaceRepository.GetPendingInvitationsByEmailAsync(
            WorkspaceEmail.Normalize(currentUser.Email),
            cancellationToken);

        return Result.Success(invitations.Select(i => i.ToResponse()).ToList());
    }
}
