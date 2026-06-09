using FastBiteGroup.Application.Abstractions.Authentication;
using FastBiteGroup.Application.Abstractions.Caching;
using FastBiteGroup.Application.Constants;
using FastBiteGroup.Application.UseCases.V1.Workspace;
using FastBiteGroup.Contract.Abstractions.Message;
using FastBiteGroup.Contract.Abstractions.Shared;
using FastBiteGroup.Contract.Services.V1.Workspace;
using FastBiteGroup.Contract.Services.V1.Workspace.Commands;
using FastBiteGroup.Contract.Services.V1.Workspace.Responses;
using FastBiteGroup.Domain.Abstractions;
using FastBiteGroup.Domain.Abstractions.Repositories;
using FastBiteGroup.Domain.Entities;
using FastBiteGroup.Domain.Enum;

namespace FastBiteGroup.Application.UseCases.V1.Commands.Workspace;

public class CreateWorkspaceCommandHandler : ICommandHandler<CreateWorkspaceCommand, WorkspaceResponse>
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly ICacheService _cacheService;

    public CreateWorkspaceCommandHandler(
        IWorkspaceRepository workspaceRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        ICacheService cacheService)
    {
        _workspaceRepository = workspaceRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _cacheService = cacheService;
    }

    public async Task<Result<WorkspaceResponse>> Handle(CreateWorkspaceCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        var workspaceId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        var workspace = new FastBiteGroup.Domain.Entities.Workspace
        {
            Id = workspaceId,
            WorkspaceName = request.WorkspaceName.Trim(),
            Description = request.Description?.Trim(),
            IsChatEnabled = request.IsChatEnabled,
            IsFeedEnabled = request.IsFeedEnabled,
            Privacy = (EnumWorkspacePrivacy)request.Privacy,
            WorkspaceAvatarUrl = request.WorkspaceAvatarUrl?.Trim() ?? string.Empty,
            CreatedByUserID = userId,
            CreatedAt = now,
            Members = new List<WorkspaceMember>
            {
                new()
                {
                    WorkspaceID = workspaceId,
                    UserID = userId,
                    Role = EnumWorkspaceRole.Owner,
                    Status = EnumWorkspaceMemberStatus.Active,
                    JoinedAt = now.UtcDateTime
                }
            }
        };

        _workspaceRepository.Add(workspace);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cacheService.RemoveAsync(CacheKeys.UserWorkspaces(userId), cancellationToken);

        var summary = await _workspaceRepository.GetWorkspaceSummaryForMemberAsync(workspaceId, userId, cancellationToken);
        return summary is null
            ? Result.Failure<WorkspaceResponse>(WorkspaceErrors.NotFound)
            : Result.Success(summary.ToResponse());
    }
}
