using FastBiteGroup.Application.Abstractions.Authentication;
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
    private readonly IJwtTokenService _jwtTokenServiceMock;
    private readonly IRefreshTokenRepository _refreshTokenRepositoryMock;
    private readonly LogoutCommandHandler _handler;

    public LogoutCommandHandlerTests()
    {
        _cacheServiceMock = Substitute.For<ICacheService>();
        _jwtTokenServiceMock = Substitute.For<IJwtTokenService>();
        _refreshTokenRepositoryMock = Substitute.For<IRefreshTokenRepository>();

        _handler = new LogoutCommandHandler(
            _cacheServiceMock,
            _jwtTokenServiceMock,
            _refreshTokenRepositoryMock);
    }

    [Fact]
    public async Task Handle_GivenValidRequest_BlacklistsJtiAndRevokesRefreshToken()
    {
        // Arrange
        const string jti = "jti-123";
        const string accessToken = "access-token";
        const string refreshToken = "refresh-token";
        var command = new AuthCommands.LogoutCommand(accessToken, jti, refreshToken, Guid.NewGuid());

        var storedRefreshToken = AppRefreshToken.Create(refreshToken, jti, Guid.NewGuid(), DateTime.UtcNow.AddDays(1));

        // Handler now uses FindSingleAsync (tracked) — single query
        _refreshTokenRepositoryMock
            .FindSingleAsync(Arg.Any<System.Linq.Expressions.Expression<Func<AppRefreshToken, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(storedRefreshToken);

        // Return a valid remaining lifetime from the token
        _jwtTokenServiceMock
            .GetAccessTokenRemainingLifetime(accessToken)
            .Returns(TimeSpan.FromMinutes(14));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Ensure JTI is blacklisted with the computed TTL
        await _cacheServiceMock.Received(1).BlacklistTokenAsync(jti, Arg.Is<TimeSpan>(t => t > TimeSpan.Zero), Arg.Any<CancellationToken>());

        // Ensure refresh token is revoked via single tracked query
        storedRefreshToken.IsRevoked.Should().BeTrue();
        _refreshTokenRepositoryMock.Received(1).Update(storedRefreshToken);

        // Ensure FindByTokenAsync is NOT called (eliminated double-query)
        await _refreshTokenRepositoryMock.DidNotReceiveWithAnyArgs().FindByTokenAsync(default!, default);
    }
}
