using FastBiteGroup.Application.Abstractions.Caching;
using FastBiteGroup.Application.UseCases.V1.Commands.Auth;
using FastBiteGroup.Contract.Services.V1.Auth.Commands;
using FastBiteGroup.Domain.Abstractions.Repositories;
using FastBiteGroup.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FastBiteGroup.Application.Tests.UseCases.V1.Commands.Auth;

public class LogoutCommandHandlerTests
{
    private readonly ICacheService _cacheServiceMock;
    private readonly IRefreshTokenRepository _refreshTokenRepositoryMock;
    private readonly LogoutCommandHandler _handler;

    public LogoutCommandHandlerTests()
    {
        _cacheServiceMock = Substitute.For<ICacheService>();
        _refreshTokenRepositoryMock = Substitute.For<IRefreshTokenRepository>();

        _handler = new LogoutCommandHandler(
            _cacheServiceMock,
            _refreshTokenRepositoryMock);
    }

    [Fact]
    public async Task Handle_GivenValidRequest_BlacklistsJtiAndRevokesRefreshToken()
    {
        // Arrange
        var command = new AuthCommands.LogoutCommand("jti-123", "refresh-token", Guid.NewGuid());
        var jti = "jti-123";

        var refreshToken = AppRefreshToken.Create("refresh-token", jti, Guid.NewGuid(), DateTime.UtcNow.AddDays(1));
        
        _refreshTokenRepositoryMock.FindByTokenAsync(command.RefreshToken, Arg.Any<CancellationToken>())
            .Returns(refreshToken);
        _refreshTokenRepositoryMock.FindSingleAsync(Arg.Any<System.Linq.Expressions.Expression<Func<AppRefreshToken, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(refreshToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        // Ensure JTI is blacklisted
        await _cacheServiceMock.Received(1).BlacklistTokenAsync(jti, Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>());
        
        // Ensure refresh token is revoked
        refreshToken.IsRevoked.Should().BeTrue();
        _refreshTokenRepositoryMock.Received(1).Update(refreshToken);
    }
}
