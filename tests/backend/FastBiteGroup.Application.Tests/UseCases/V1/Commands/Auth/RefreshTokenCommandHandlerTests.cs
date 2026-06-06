using FastBiteGroup.Application.Abstractions.Authentication;
using FastBiteGroup.Application.UseCases.V1.Commands.Auth;
using FastBiteGroup.Contract.Services.V1.Auth.Commands;
using FastBiteGroup.Domain.Abstractions.Repositories;
using FastBiteGroup.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FastBiteGroup.Application.Tests.UseCases.V1.Commands.Auth;

public class RefreshTokenCommandHandlerTests
{
    private readonly IJwtTokenService _jwtTokenServiceMock;
    private readonly IRefreshTokenRepository _refreshTokenRepositoryMock;
    private readonly IUserAuthService _userAuthServiceMock;
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _jwtTokenServiceMock = Substitute.For<IJwtTokenService>();
        _refreshTokenRepositoryMock = Substitute.For<IRefreshTokenRepository>();
        _userAuthServiceMock = Substitute.For<IUserAuthService>();

        _handler = new RefreshTokenCommandHandler(
            _jwtTokenServiceMock,
            _refreshTokenRepositoryMock,
            _userAuthServiceMock);
    }

    [Fact]
    public async Task Handle_GivenValidTokens_ReturnsNewTokens()
    {
        // Arrange
        var command = new AuthCommands.RefreshTokenCommand("old-access", "old-refresh");
        var userId = Guid.NewGuid();
        var jti = "jti-123";

        var oldRefreshToken = AppRefreshToken.Create("old-refresh", jti, userId, DateTime.UtcNow.AddDays(1));

        var userDto = new UserDto(
            Id: userId,
            Email: "test@test.com",
            UserName: "test@test.com",
            FirstName: "First",
            LastName: "Last",
            FullName: "First Last",
            AvatarUrl: null,
            Bio: null,
            IsActive: true,
            LastSeenAt: null,
            Roles: new List<string> { "Customer" });

        _jwtTokenServiceMock.GetJtiFromExpiredToken(command.AccessToken).Returns(jti);

        // Handler now uses FindSingleAsync (tracked) for single-query pattern
        _refreshTokenRepositoryMock
            .FindSingleAsync(Arg.Any<System.Linq.Expressions.Expression<Func<AppRefreshToken, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(oldRefreshToken);

        _userAuthServiceMock.FindByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(userDto);

        _jwtTokenServiceMock.GenerateAccessToken(userId, userDto.Email, userDto.UserName, userDto.FirstName, userDto.LastName, userDto.Roles)
            .Returns(("new-access", "new-jti", DateTime.UtcNow.AddMinutes(15)));
        _jwtTokenServiceMock.GenerateRefreshToken().Returns("new-refresh");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("new-access");
        result.Value.RefreshToken.Should().Be("new-refresh");
        result.Value.TokenType.Should().Be("Bearer");
        result.Value.User.IsActive.Should().BeTrue();

        oldRefreshToken.IsUsed.Should().BeTrue();
        _refreshTokenRepositoryMock.Received(1).Update(oldRefreshToken);
        _refreshTokenRepositoryMock.Received(1).Add(Arg.Is<AppRefreshToken>(r => r.Token == "new-refresh" && r.Jti == "new-jti"));

        // Ensure FindByTokenAsync is NOT called (eliminated double-query)
        await _refreshTokenRepositoryMock.DidNotReceiveWithAnyArgs().FindByTokenAsync(default!, default);
    }

    [Fact]
    public async Task Handle_GivenInvalidAccessTokenJti_ReturnsFailure()
    {
        // Arrange
        var command = new AuthCommands.RefreshTokenCommand("invalid-access", "old-refresh");
        _jwtTokenServiceMock.GetJtiFromExpiredToken(command.AccessToken).Returns((string?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.InvalidToken");
    }

    [Fact]
    public async Task Handle_GivenExpiredOrRevokedRefreshToken_ReturnsFailure()
    {
        // Arrange
        var command = new AuthCommands.RefreshTokenCommand("old-access", "old-refresh");
        var jti = "jti-123";
        _jwtTokenServiceMock.GetJtiFromExpiredToken(command.AccessToken).Returns(jti);

        var revokedToken = AppRefreshToken.Create("old-refresh", jti, Guid.NewGuid(), DateTime.UtcNow.AddDays(1));
        revokedToken.Revoke(); // mark as revoked

        // Handler uses FindSingleAsync
        _refreshTokenRepositoryMock
            .FindSingleAsync(Arg.Any<System.Linq.Expressions.Expression<Func<AppRefreshToken, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(revokedToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.RefreshTokenExpiredOrRevoked");
    }

    [Fact]
    public async Task Handle_GivenMismatchedJti_ReturnsFailure()
    {
        // Arrange
        var command = new AuthCommands.RefreshTokenCommand("old-access", "old-refresh");
        var jtiFromAccess = "jti-access";
        var jtiFromRefresh = "jti-refresh";

        _jwtTokenServiceMock.GetJtiFromExpiredToken(command.AccessToken).Returns(jtiFromAccess);

        var refreshToken = AppRefreshToken.Create("old-refresh", jtiFromRefresh, Guid.NewGuid(), DateTime.UtcNow.AddDays(1));

        // Handler uses FindSingleAsync
        _refreshTokenRepositoryMock
            .FindSingleAsync(Arg.Any<System.Linq.Expressions.Expression<Func<AppRefreshToken, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(refreshToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.TokenMismatch");
    }
}
