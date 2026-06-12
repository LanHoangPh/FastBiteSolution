using FastBiteGroup.Application.Abstractions.Authentication;
using FastBiteGroup.Application.Abstractions.Caching;
using FastBiteGroup.Application.UseCases.V1.Commands.Workspace;
using FastBiteGroup.Contract.Services.V1.Workspace;
using FastBiteGroup.Contract.Services.V1.Workspace.Commands;
using FastBiteGroup.Domain.Abstractions;
using FastBiteGroup.Domain.Abstractions.Repositories;
using FastBiteGroup.Domain.Entities;
using FastBiteGroup.Domain.Enums;
using FluentAssertions;
using NSubstitute;

namespace FastBiteGroup.Application.Tests.UseCases.V1.Commands.Workspace;

public class WorkspaceCommandHandlersTests
{
    [Fact]
    public async Task UpdateWorkspace_WhenWorkspaceChanges_RemovesCacheForAllActiveMembers()
    {
        var workspaceId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var repository = Substitute.For<IWorkspaceRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUser>();
        var cache = Substitute.For<ICacheService>();
        var workspace = new FastBiteGroup.Domain.Entities.Workspace
        {
            Id = workspaceId,
            WorkspaceName = "Acme",
            Privacy = EnumWorkspacePrivacy.Private,
            Members = new List<WorkspaceMember>
            {
                new()
                {
                    WorkspaceID = workspaceId,
                    UserID = ownerId,
                    Role = EnumWorkspaceRole.Owner,
                    Status = EnumWorkspaceMemberStatus.Active
                },
                new()
                {
                    WorkspaceID = workspaceId,
                    UserID = memberId,
                    Role = EnumWorkspaceRole.Member,
                    Status = EnumWorkspaceMemberStatus.Active
                }
            }
        };

        currentUser.UserId.Returns(ownerId);
        repository.GetActiveMemberAsync(workspaceId, ownerId, Arg.Any<CancellationToken>())
            .Returns(new WorkspaceMember { WorkspaceID = workspaceId, UserID = ownerId, Role = EnumWorkspaceRole.Owner, Status = EnumWorkspaceMemberStatus.Active });
        repository.GetWorkspaceForUpdateAsync(workspaceId, Arg.Any<CancellationToken>())
            .Returns(workspace);
        repository.GetWorkspaceSummaryForMemberAsync(workspaceId, ownerId, Arg.Any<CancellationToken>())
            .Returns(new WorkspaceSummary(
                workspaceId,
                "Acme Updated",
                null,
                true,
                false,
                EnumWorkspacePrivacy.Private,
                string.Empty,
                EnumWorkspaceRole.Owner,
                DateTimeOffset.UtcNow,
                false,
                2));

        var handler = new UpdateWorkspaceCommandHandler(repository, unitOfWork, currentUser, cache);

        var result = await handler.Handle(
            new UpdateWorkspaceCommand(workspaceId, "Acme Updated", null, true, false, 2, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await cache.Received(1).RemoveAsync(Application.Constants.CacheKeys.UserWorkspaces(ownerId), Arg.Any<CancellationToken>());
        await cache.Received(1).RemoveAsync(Application.Constants.CacheKeys.UserWorkspaces(memberId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InviteWorkspaceMember_WhenPendingInvitationExists_ReturnsConflict()
    {
        var userId = Guid.NewGuid();
        var Id = Guid.NewGuid();
        var repository = Substitute.For<IWorkspaceRepository>();
        var userAuthService = Substitute.For<IUserAuthService>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUser>();

        currentUser.UserId.Returns(userId);
        repository.GetActiveMemberAsync(Id, userId, Arg.Any<CancellationToken>())
            .Returns(new WorkspaceMember { WorkspaceID = Id, UserID = userId, Role = EnumWorkspaceRole.Admin, Status = EnumWorkspaceMemberStatus.Active });
        repository.GetWorkspaceForUpdateAsync(Id, Arg.Any<CancellationToken>())
            .Returns(new FastBiteGroup.Domain.Entities.Workspace { Id = Id, WorkspaceName = "Acme" });
        repository.HasPendingInvitationAsync(Id, "dev@acme.com", Arg.Any<CancellationToken>())
            .Returns(true);

        var handler = new InviteWorkspaceMemberCommandHandler(repository, userAuthService, unitOfWork, currentUser);

        var result = await handler.Handle(new InviteWorkspaceMemberCommand(Id, "Dev@Acme.com"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(WorkspaceErrors.InvitationAlreadyExists);
        repository.DidNotReceive().AddUserInvitation(Arg.Any<UserWorkspaceInvitation>());
    }

    [Fact]
    public async Task AcceptWorkspaceInvitation_WhenEmailDoesNotMatch_ReturnsInvitationNotFound()
    {
        var userId = Guid.NewGuid();
        var repository = Substitute.For<IWorkspaceRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUser>();
        var cache = Substitute.For<ICacheService>();

        currentUser.UserId.Returns(userId);
        currentUser.Email.Returns("user@acme.com");
        repository.GetPendingUserInvitationForUpdateAsync(10, "user@acme.com", Arg.Any<CancellationToken>())
            .Returns((UserWorkspaceInvitation?)null);

        var handler = new AcceptWorkspaceInvitationCommandHandler(repository, unitOfWork, currentUser, cache);

        var result = await handler.Handle(new AcceptWorkspaceInvitationCommand(10), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(WorkspaceErrors.InvitationNotFound);
    }

    [Fact]
    public async Task JoinWorkspace_WhenInviteCodeExpired_ReturnsExpired()
    {
        var Id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var repository = Substitute.For<IWorkspaceRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUser>();
        var cache = Substitute.For<ICacheService>();

        currentUser.UserId.Returns(userId);
        repository.GetWorkspaceInvitationByCodeForUpdateAsync("ABC", Arg.Any<CancellationToken>())
            .Returns(new WorkspaceInvitation
            {
                WorkspaceID = Id,
                InvitationCode = "ABC",
                IsActive = true,
                ExpiresAt = DateTime.UtcNow.AddMinutes(-1),
                Workspace = new FastBiteGroup.Domain.Entities.Workspace { Id = Id }
            });

        var handler = new JoinWorkspaceCommandHandler(repository, unitOfWork, currentUser, cache);

        var result = await handler.Handle(new JoinWorkspaceCommand("ABC"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(WorkspaceErrors.InvitationExpired);
    }

    [Fact]
    public async Task JoinWorkspace_WhenInviteCodeIsLowercase_LooksUpUppercaseCode()
    {
        var repository = Substitute.For<IWorkspaceRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUser>();
        var cache = Substitute.For<ICacheService>();

        currentUser.UserId.Returns(Guid.NewGuid());

        var handler = new JoinWorkspaceCommandHandler(repository, unitOfWork, currentUser, cache);

        await handler.Handle(new JoinWorkspaceCommand("abc123"), CancellationToken.None);

        await repository.Received(1).GetWorkspaceInvitationByCodeForUpdateAsync("ABC123", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ArchiveWorkspace_WhenCurrentUserIsNotOwner_ReturnsForbidden()
    {
        var Id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var repository = Substitute.For<IWorkspaceRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUser>();
        var cache = Substitute.For<ICacheService>();

        currentUser.UserId.Returns(userId);
        repository.GetActiveMemberAsync(Id, userId, Arg.Any<CancellationToken>())
            .Returns(new WorkspaceMember { WorkspaceID = Id, UserID = userId, Role = EnumWorkspaceRole.Admin, Status = EnumWorkspaceMemberStatus.Active });

        var handler = new ArchiveWorkspaceCommandHandler(repository, unitOfWork, currentUser, cache);

        var result = await handler.Handle(new ArchiveWorkspaceCommand(Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(WorkspaceErrors.Forbidden);
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
