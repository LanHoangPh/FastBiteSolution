using FastBiteGroup.Contract.Abstractions.Message;
using FastBiteGroup.Contract.Abstractions.Shared;
using FastBiteGroup.Contract.Services.V1.Workspace.Commands;
using FastBiteGroup.Domain.Abstractions;
using FastBiteGroup.Domain.Abstractions.Repositories;
using FastBiteGroup.Domain.Entities;
using FastBiteGroup.Domain.Enum;
using FastBiteGroup.Application.Abstractions.Authentication;

namespace FastBiteGroup.Application.UseCases.V1.Commands.Workspace;

public class CreateWorkspaceCommandHandler : ICommandHandler<CreateWorkspaceCommand>
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public CreateWorkspaceCommandHandler(
        IWorkspaceRepository workspaceRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _workspaceRepository = workspaceRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(CreateWorkspaceCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        // Create Workspace Entity
        var workspace = new FastBiteGroup.Domain.Entities.Workspace
        {
            WorkspaceName = request.WorkspaceName,
            Description = request.Description,
            WorkspaceType = (EnumWorkspaceType)request.WorkspaceType,
            Privacy = (EnumWorkspacePrivacy)request.Privacy,
            WorkspaceAvatarUrl = request.WorkspaceAvatarUrl ?? string.Empty,
            CreatedByUserID = userId,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        // Assign Creator as Owner
        var member = new WorkspaceMember
        {
            UserID = userId,
            Role = EnumWorkspaceRole.Owner,
            JoinedAt = DateTime.UtcNow
        };
        workspace.Members = new List<WorkspaceMember> { member };

        _workspaceRepository.Add(workspace);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
