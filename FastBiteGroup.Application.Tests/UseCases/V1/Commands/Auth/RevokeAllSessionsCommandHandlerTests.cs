using FastBiteGroup.Application.UseCases.V1.Commands.Auth;
using FastBiteGroup.Contract.Services.V1.Auth.Commands;
using FastBiteGroup.Domain.Abstractions.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FastBiteGroup.Application.Tests.UseCases.V1.Commands.Auth;

public class RevokeAllSessionsCommandHandlerTests
{
    private readonly IRefreshTokenRepository _refreshTokenRepositoryMock;
    private readonly RevokeAllSessionsCommandHandler _handler;

    public RevokeAllSessionsCommandHandlerTests()
    {
        _refreshTokenRepositoryMock = Substitute.For<IRefreshTokenRepository>();
        _handler = new RevokeAllSessionsCommandHandler(_refreshTokenRepositoryMock);
    }

    [Fact]
    public async Task Handle_GivenValidUserId_RevokesAllSessions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new AuthCommands.RevokeAllSessionsCommand(userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _refreshTokenRepositoryMock.Received(1).RevokeAllForUserAsync(userId, Arg.Any<CancellationToken>());
    }
}
