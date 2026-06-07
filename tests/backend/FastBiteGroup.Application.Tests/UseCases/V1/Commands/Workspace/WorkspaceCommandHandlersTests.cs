using FastBiteGroup.Application.Abstractions.Authentication;
using FastBiteGroup.Application.Abstractions.Caching;
using FastBiteGroup.Application.UseCases.V1.Commands.Workspace;
using FastBiteGroup.Contract.Services.V1.Workspace;
using FastBiteGroup.Contract.Services.V1.Workspace.Commands;
using FastBiteGroup.Domain.Abstractions;
using FastBiteGroup.Domain.Abstractions.Repositories;
using FastBiteGroup.Domain.Entities;
using FastBiteGroup.Domain.Enum;
using FluentAssertions;
using NSubstitute;

namespace FastBiteGroup.Application.Tests.UseCases.V1.Commands.Workspace;

public class WorkspaceCommandHandlersTests
{
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
