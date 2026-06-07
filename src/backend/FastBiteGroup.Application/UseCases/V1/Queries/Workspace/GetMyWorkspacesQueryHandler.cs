using FastBiteGroup.Contract.Abstractions.Message;
using FastBiteGroup.Contract.Abstractions.Shared;
using FastBiteGroup.Contract.Services.V1.Workspace.Queries;
using FastBiteGroup.Contract.Services.V1.Workspace.Responses;
using FastBiteGroup.Domain.Abstractions.Repositories;
using FastBiteGroup.Application.Abstractions.Authentication;

namespace FastBiteGroup.Application.UseCases.V1.Queries.Workspace;

public class GetMyWorkspacesQueryHandler : IQueryHandler<GetMyWorkspacesQuery, List<WorkspaceResponse>>
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly ICurrentUser _currentUser;

    public GetMyWorkspacesQueryHandler(
        IWorkspaceRepository workspaceRepository,
        ICurrentUser currentUser)
    {
        _workspaceRepository = workspaceRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<List<WorkspaceResponse>>> Handle(GetMyWorkspacesQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        var workspaces = await _workspaceRepository.GetWorkspacesByUserIdAsync(userId, cancellationToken);

        var response = workspaces.Select(w =>
        {
            var currentUserMember = w.Members.FirstOrDefault(m => m.UserID == userId && m.LeftAt == null);
            var role = currentUserMember?.Role.ToString() ?? "Unknown";

            return new WorkspaceResponse(
                w.WorkspaceID,
                w.WorkspaceName,
                w.Description,
                w.WorkspaceType.ToString(),
                w.Privacy.ToString(),
                w.WorkspaceAvatarUrl ?? string.Empty,
                role
            );
        }).ToList();

        return Result.Success(response);
    }
}
