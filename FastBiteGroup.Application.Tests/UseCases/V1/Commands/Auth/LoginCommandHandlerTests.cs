using FastBiteGroup.Application.Abstractions.Authentication;
using FastBiteGroup.Application.UseCases.V1.Commands.Auth;
using FastBiteGroup.Contract.Services.V1.Auth.Commands;
using FastBiteGroup.Domain.Abstractions.Repositories;
using FastBiteGroup.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FastBiteGroup.Application.Tests.UseCases.V1.Commands.Auth;

public class LoginCommandHandlerTests
{
    private readonly IUserAuthService _userAuthServiceMock;
    private readonly IJwtTokenService _jwtTokenServiceMock;
    private readonly IRefreshTokenRepository _refreshTokenRepositoryMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _userAuthServiceMock = Substitute.For<IUserAuthService>();
        _jwtTokenServiceMock = Substitute.For<IJwtTokenService>();
        _refreshTokenRepositoryMock = Substitute.For<IRefreshTokenRepository>();

        _handler = new LoginCommandHandler(
            _userAuthServiceMock,
            _jwtTokenServiceMock,
            _refreshTokenRepositoryMock);
    }

    [Fact]
    public async Task Handle_GivenValidCredentials_ReturnsSuccessWithTokens()
    {
        // Arrange
        var command = new AuthCommands.LoginCommand("test@test.com", "Password123!");
        var userId = Guid.NewGuid();
        var userDto = new UserDto(userId, command.Email, command.Email, "First", "Last", new List<string> { "Customer" });

        _userAuthServiceMock.FindByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(userDto);
        _userAuthServiceMock.IsLockedOutAsync(userId, Arg.Any<CancellationToken>())
            .Returns(false);
        _userAuthServiceMock.CheckPasswordAsync(userId, command.Password, Arg.Any<CancellationToken>())
            .Returns(true);

        _jwtTokenServiceMock.GenerateAccessToken(userId, userDto.Email, userDto.UserName, userDto.FirstName, userDto.LastName, userDto.Roles)
            .Returns(("access-token", "jti-123", DateTime.UtcNow.AddMinutes(15)));
        _jwtTokenServiceMock.GenerateRefreshToken()
            .Returns("refresh-token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("access-token");
        result.Value.RefreshToken.Should().Be("refresh-token");
        _refreshTokenRepositoryMock.Received(1).Add(Arg.Is<AppRefreshToken>(r => r.UserId == userId && r.Token == "refresh-token"));
    }

    [Fact]
    public async Task Handle_GivenInvalidEmail_ReturnsFailure()
    {
        // Arrange
        var command = new AuthCommands.LoginCommand("wrong@test.com", "Password123!");
        _userAuthServiceMock.FindByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns((UserDto?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.InvalidCredentials");
    }

    [Fact]
    public async Task Handle_GivenInvalidPassword_ReturnsFailure()
    {
        // Arrange
        var command = new AuthCommands.LoginCommand("test@test.com", "WrongPassword!");
        var userId = Guid.NewGuid();
        var userDto = new UserDto(userId, command.Email, command.Email, "First", "Last", new List<string> { "Customer" });

        _userAuthServiceMock.FindByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(userDto);
        _userAuthServiceMock.IsLockedOutAsync(userId, Arg.Any<CancellationToken>())
            .Returns(false);
        _userAuthServiceMock.CheckPasswordAsync(userId, command.Password, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.InvalidCredentials");
    }

    [Fact]
    public async Task Handle_GivenLockedOutUser_ReturnsFailure()
    {
        // Arrange
        var command = new AuthCommands.LoginCommand("test@test.com", "Password123!");
        var userId = Guid.NewGuid();
        var userDto = new UserDto(userId, command.Email, command.Email, "First", "Last", new List<string> { "Customer" });

        _userAuthServiceMock.FindByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(userDto);
        _userAuthServiceMock.IsLockedOutAsync(userId, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.AccountLocked");
    }
}
