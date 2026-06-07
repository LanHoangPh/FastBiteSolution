using FastBiteGroup.Application.Abstractions.Authentication;
using FastBiteGroup.Application.Abstractions.Caching;
using FastBiteGroup.Application.Constants;
using FastBiteGroup.Application.UseCases.V1.Workspace;
using FastBiteGroup.Contract.Abstractions.Message;
using FastBiteGroup.Contract.Abstractions.Shared;
using FastBiteGroup.Contract.Services.V1.Workspace.Queries;
using FastBiteGroup.Contract.Services.V1.Workspace.Responses;
using FastBiteGroup.Domain.Abstractions.Repositories;

namespace FastBiteGroup.Application.UseCases.V1.Queries.Workspace;

public class GetMyWorkspacesQueryHandler : IQueryHandler<GetMyWorkspacesQuery, List<WorkspaceResponse>>
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly ICurrentUser _currentUser;
    private readonly ICacheService _cacheService;

    public GetMyWorkspacesQueryHandler(
        IWorkspaceRepository workspaceRepository,
        ICurrentUser currentUser,
        ICacheService cacheService)
    {
        _workspaceRepository = workspaceRepository;
        _currentUser = currentUser;
        _cacheService = cacheService;
    }

    public async Task<Result<List<WorkspaceResponse>>> Handle(GetMyWorkspacesQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.UserWorkspaces(_currentUser.UserId);
        var cached = await _cacheService.GetAsync<List<WorkspaceResponse>>(cacheKey, cancellationToken);
        if (cached is not null)
            return Result.Success(cached);

        var workspaces = await _workspaceRepository.GetActiveWorkspacesByUserIdAsync(_currentUser.UserId, cancellationToken);
        var response = workspaces.Select(w => w.ToResponse()).ToList();

        await _cacheService.SetAsync(cacheKey, response, CacheDuration, cancellationToken);
        return Result.Success(response);
    }
}
