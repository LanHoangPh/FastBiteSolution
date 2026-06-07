using FastBiteGroup.Application.Abstractions.Authentication;
using FastBiteGroup.Application.Abstractions.Caching;
using FastBiteGroup.Application.UseCases.V1.Queries.Workspace;
using FastBiteGroup.Contract.Services.V1.Workspace;
using FastBiteGroup.Contract.Services.V1.Workspace.Queries;
using FastBiteGroup.Contract.Services.V1.Workspace.Responses;
using FastBiteGroup.Domain.Abstractions.Repositories;
using FastBiteGroup.Domain.Entities;
using FastBiteGroup.Domain.Enum;
using FluentAssertions;
using NSubstitute;

namespace FastBiteGroup.Application.Tests.UseCases.V1.Queries.Workspace;

public class WorkspaceQueryHandlersTests
{
    [Fact]
    public async Task GetMyWorkspaces_WhenCacheHit_ReturnsCachedResponse()
    {
        var userId = Guid.NewGuid();
        var repository = Substitute.For<IWorkspaceRepository>();
        var currentUser = Substitute.For<ICurrentUser>();
        var cache = Substitute.For<ICacheService>();
        var cached = new List<WorkspaceResponse>
        {
            new(Guid.NewGuid(), "Cached", null, "Private", "Private", string.Empty, "Owner", DateTimeOffset.UtcNow, false, 1)
        };

        currentUser.UserId.Returns(userId);
        cache.GetAsync<List<WorkspaceResponse>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(cached);

        var handler = new GetMyWorkspacesQueryHandler(repository, currentUser, cache);

        var result = await handler.Handle(new GetMyWorkspacesQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(cached);
        await repository.DidNotReceive().GetActiveWorkspacesByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetWorkspaceById_WhenUserIsNotMember_ReturnsForbidden()
    {
        var workspaceId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var repository = Substitute.For<IWorkspaceRepository>();
        var currentUser = Substitute.For<ICurrentUser>();

        currentUser.UserId.Returns(userId);
        repository.GetWorkspaceSummaryForMemberAsync(workspaceId, userId, Arg.Any<CancellationToken>())
            .Returns((WorkspaceSummary?)null);

        var handler = new GetWorkspaceByIdQueryHandler(repository, currentUser);

        var result = await handler.Handle(new GetWorkspaceByIdQuery(workspaceId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(WorkspaceErrors.Forbidden);
    }

    [Fact]
    public async Task GetWorkspaceMembers_WhenUserIsMember_ReturnsMembers()
    {
        var workspaceId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var repository = Substitute.For<IWorkspaceRepository>();
        var currentUser = Substitute.For<ICurrentUser>();

        currentUser.UserId.Returns(userId);
        repository.GetActiveMemberAsync(workspaceId, userId, Arg.Any<CancellationToken>())
            .Returns(new WorkspaceMember { WorkspaceID = workspaceId, UserID = userId, Status = EnumWorkspaceMemberStatus.Active });
        repository.GetActiveMembersAsync(workspaceId, Arg.Any<CancellationToken>())
            .Returns(new List<WorkspaceMemberSummary>
            {
                new(1, userId, "user@acme.com", "User Acme", null, EnumWorkspaceRole.Owner, EnumWorkspaceMemberStatus.Active, DateTime.UtcNow)
            });

        var handler = new GetWorkspaceMembersQueryHandler(repository, currentUser);

        var result = await handler.Handle(new GetWorkspaceMembersQuery(workspaceId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle(m => m.Email == "user@acme.com" && m.Role == "Owner");
    }
}
