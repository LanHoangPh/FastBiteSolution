using FastBiteGroup.Application.Abstractions.Authentication;
using FastBiteGroup.Application.Abstractions.Caching;
using FastBiteGroup.Application.Constants;
using FastBiteGroup.Application.UseCases.V1.Commands.Workspace;
using FastBiteGroup.Contract.Services.V1.Workspace.Commands;
using FastBiteGroup.Domain.Abstractions;
using FastBiteGroup.Domain.Abstractions.Repositories;
using FastBiteGroup.Domain.Entities;
using FastBiteGroup.Domain.Enum;
using FluentAssertions;
using NSubstitute;

namespace FastBiteGroup.Application.Tests.UseCases.V1.Commands.Workspace;

public class CreateWorkspaceCommandHandlerTests
{
    [Fact]
    public async Task Handle_GivenValidRequest_CreatesWorkspaceWithOwnerAndReturnsResponse()
    {
        var userId = Guid.NewGuid();
        var repository = Substitute.For<IWorkspaceRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUser>();
        var cache = Substitute.For<ICacheService>();
        FastBiteGroup.Domain.Entities.Workspace? capturedWorkspace = null;

        currentUser.UserId.Returns(userId);
        repository.When(x => x.Add(Arg.Any<FastBiteGroup.Domain.Entities.Workspace>()))
            .Do(call => capturedWorkspace = call.Arg<FastBiteGroup.Domain.Entities.Workspace>());

        repository.GetWorkspaceSummaryForMemberAsync(
                Arg.Any<Guid>(),
                userId,
                Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var workspaceId = call.ArgAt<Guid>(0);
                return new WorkspaceSummary(
                    workspaceId,
                    "Acme",
                    "Team",
                    EnumWorkspaceType.Private,
                    EnumWorkspacePrivacy.Private,
                    string.Empty,
                    EnumWorkspaceRole.Owner,
                    DateTimeOffset.UtcNow,
                    false,
                    1);
            });

        var handler = new CreateWorkspaceCommandHandler(repository, unitOfWork, currentUser, cache);
        var command = new CreateWorkspaceCommand("Acme", "Team", 1, 2, null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.CurrentUserRole.Should().Be("Owner");
        capturedWorkspace.Should().NotBeNull();
        capturedWorkspace!.Id.Should().NotBeEmpty();
        capturedWorkspace.Members.Should().ContainSingle(m =>
            m.UserID == userId &&
            m.Role == EnumWorkspaceRole.Owner &&
            m.Status == EnumWorkspaceMemberStatus.Active);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await cache.Received(1).RemoveAsync(CacheKeys.UserWorkspaces(userId), Arg.Any<CancellationToken>());
    }
}
