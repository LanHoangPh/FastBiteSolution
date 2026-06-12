using FastBiteGroup.Application.UseCases.V1.Workspace;
using FastBiteGroup.Contract.Services.V1.Workspace;
using FastBiteGroup.Contract.Services.V1.Workspace.Queries;
using FastBiteGroup.Contract.Services.V1.Workspace.Responses;

namespace FastBiteGroup.Application.UseCases.V1.Queries.Workspace;

public sealed class GetWorkspaceMembersQueryHandler(IWorkspaceRepository workspaceRepository, ICurrentUser currentUser)
    : IQueryHandler<GetWorkspaceMembersQuery, List<WorkspaceMemberResponse>>
{
    public async Task<Result<List<WorkspaceMemberResponse>>> Handle(GetWorkspaceMembersQuery request, CancellationToken cancellationToken)
    {
        var workspace = await workspaceRepository.GetWorkspaceSummaryForMemberAsync(
            request.WorkspaceId,
            currentUser.UserId,
            cancellationToken);
        if (workspace is null)
            return Result.Failure<List<WorkspaceMemberResponse>>(WorkspaceErrors.Forbidden);

        var members = await workspaceRepository.GetActiveMembersAsync(request.WorkspaceId, cancellationToken);
        return Result.Success(members.Select(m => m.ToResponse()).ToList());
    }
}
