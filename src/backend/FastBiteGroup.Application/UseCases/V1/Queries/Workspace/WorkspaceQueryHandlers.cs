using FastBiteGroup.Application.UseCases.V1.Workspace;
using FastBiteGroup.Contract.Services.V1.Workspace;
using FastBiteGroup.Contract.Services.V1.Workspace.Queries;
using FastBiteGroup.Contract.Services.V1.Workspace.Responses;

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
