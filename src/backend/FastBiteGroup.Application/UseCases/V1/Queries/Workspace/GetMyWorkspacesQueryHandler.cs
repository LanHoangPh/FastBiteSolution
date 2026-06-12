using FastBiteGroup.Application.Constants;
using FastBiteGroup.Application.UseCases.V1.Workspace;
using FastBiteGroup.Contract.Services.V1.Workspace.Queries;
using FastBiteGroup.Contract.Services.V1.Workspace.Responses;

namespace FastBiteGroup.Application.UseCases.V1.Queries.Workspace;

public class GetMyWorkspacesQueryHandler(
    IWorkspaceRepository workspaceRepository,
    ICurrentUser currentUser,
    ICacheService cacheService)
    : IQueryHandler<GetMyWorkspacesQuery, List<WorkspaceResponse>>
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public async Task<Result<List<WorkspaceResponse>>> Handle(GetMyWorkspacesQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.UserWorkspaces(currentUser.UserId);
        var cached = await cacheService.GetAsync<List<WorkspaceResponse>>(cacheKey, cancellationToken);
        if (cached is not null)
            return Result.Success(cached);

        var workspaces = await workspaceRepository.GetActiveWorkspacesByUserIdAsync(currentUser.UserId, cancellationToken);
        var response = workspaces.Select(w => w.ToResponse()).ToList();

        await cacheService.SetAsync(cacheKey, response, CacheDuration, cancellationToken);
        return Result.Success(response);
    }
}
