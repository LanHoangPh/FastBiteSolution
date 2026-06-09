using FastBiteGroup.Application.UseCases.V1.Workspace;
using FastBiteGroup.Contract.Services.V1.Workspace;
using FastBiteGroup.Contract.Services.V1.Workspace.Queries;
using FastBiteGroup.Contract.Services.V1.Workspace.Responses;

namespace FastBiteGroup.Application.UseCases.V1.Queries.Workspace;

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
        var workspace = await _workspaceRepository.GetWorkspaceSummaryForMemberAsync(
            request.WorkspaceId,
            _currentUser.UserId,
            cancellationToken);
        if (workspace is null)
            return Result.Failure<List<WorkspaceMemberResponse>>(WorkspaceErrors.Forbidden);

        var members = await _workspaceRepository.GetActiveMembersAsync(request.WorkspaceId, cancellationToken);
        return Result.Success(members.Select(m => m.ToResponse()).ToList());
    }
}
