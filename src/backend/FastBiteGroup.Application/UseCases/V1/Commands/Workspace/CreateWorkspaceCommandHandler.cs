using FastBiteGroup.Application.Constants;
using FastBiteGroup.Application.UseCases.V1.Workspace;
using FastBiteGroup.Contract.Services.V1.Workspace;
using FastBiteGroup.Contract.Services.V1.Workspace.Commands;
using FastBiteGroup.Contract.Services.V1.Workspace.Responses;
using FastBiteGroup.Domain.Abstractions;
using FastBiteGroup.Domain.Enums;

namespace FastBiteGroup.Application.UseCases.V1.Commands.Workspace;

public class CreateWorkspaceCommandHandler(
    IWorkspaceRepository workspaceRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    ICacheService cacheService)
    : ICommandHandler<CreateWorkspaceCommand, WorkspaceResponse>
{
    public async Task<Result<WorkspaceResponse>> Handle(CreateWorkspaceCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId;
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

        workspaceRepository.Add(workspace);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await cacheService.RemoveAsync(CacheKeys.UserWorkspaces(userId), cancellationToken);

        var summary = await workspaceRepository.GetWorkspaceSummaryForMemberAsync(workspaceId, userId, cancellationToken);
        return summary is null
            ? Result.Failure<WorkspaceResponse>(WorkspaceErrors.NotFound)
            : Result.Success(summary.ToResponse());
    }
}
