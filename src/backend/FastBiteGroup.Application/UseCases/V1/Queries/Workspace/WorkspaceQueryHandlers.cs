using FastBiteGroup.Application.UseCases.V1.Workspace;
using FastBiteGroup.Contract.Services.V1.Workspace;
using FastBiteGroup.Contract.Services.V1.Workspace.Queries;
using FastBiteGroup.Contract.Services.V1.Workspace.Responses;

namespace FastBiteGroup.Application.UseCases.V1.Queries.Workspace;

public sealed class GetWorkspaceByIdQueryHandler(IWorkspaceRepository workspaceRepository, ICurrentUser currentUser)
    : IQueryHandler<GetWorkspaceByIdQuery, WorkspaceDetailResponse>
{
    public async Task<Result<WorkspaceDetailResponse>> Handle(GetWorkspaceByIdQuery request, CancellationToken cancellationToken)
    {
        var summary = await workspaceRepository.GetWorkspaceSummaryForMemberAsync(
            request.WorkspaceId,
            currentUser.UserId,
            cancellationToken);

        return summary is null
            ? Result.Failure<WorkspaceDetailResponse>(WorkspaceErrors.Forbidden)
            : Result.Success(summary.ToDetailResponse());
    }
}
