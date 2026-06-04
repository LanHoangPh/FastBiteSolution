using FastBiteGroup.Application.UseCases.V1.Commands.Auth;
using FastBiteGroup.Application.Tests.Common.Assertions;
using FastBiteGroup.Application.Tests.Common.Builders;
using FastBiteGroup.Application.Tests.Common.Fixtures;
using FastBiteGroup.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FastBiteGroup.Application.Tests.UseCases.V1.Commands.Auth;

public class LoginCommandHandlerTests
{
    private readonly AuthHandlerFixture _fixture;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _fixture = new AuthHandlerFixture();

        _handler = new LoginCommandHandler(
            _fixture.UserAuthService,
            _fixture.JwtTokenService,
            _fixture.RefreshTokenRepository);
    }

    [Fact]
    public async Task Handle_GivenValidCredentials_ReturnsSuccessWithTokens()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = AuthTestData.LoginCommand();
        var userDto = AuthTestData.User(id: userId, email: command.Email);

        _fixture.GivenUserFoundByEmail(userDto);
        _fixture.GivenUserIsNotLocked(userId);
        _fixture.GivenPasswordIsValid(userId);
        _fixture.GivenTokenPairFor(
            userDto,
            AuthTestData.AccessToken,
            AuthTestData.Jti,
            DateTime.UtcNow.AddMinutes(15),
            AuthTestData.RefreshToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var response = result.ShouldBeSuccess();
        response.AccessToken.Should().Be(AuthTestData.AccessToken);
        response.RefreshToken.Should().Be(AuthTestData.RefreshToken);
        _fixture.RefreshTokenRepository.Received(1).Add(
            Arg.Is<AppRefreshToken>(r => r.UserId == userId && r.Token == AuthTestData.RefreshToken));
    }

    [Fact]
    public async Task Handle_GivenInvalidEmail_ReturnsFailure()
    {
        // Arrange
        var command = AuthTestData.LoginCommand(email: "wrong@test.com");
        _fixture.GivenUserFoundByEmail(null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldFailWith("Auth.InvalidCredentials");
    }

    [Fact]
    public async Task Handle_GivenInvalidPassword_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = AuthTestData.LoginCommand(password: "WrongPassword!");
        var userDto = AuthTestData.User(id: userId, email: command.Email);

        _fixture.GivenUserFoundByEmail(userDto);
        _fixture.GivenUserIsNotLocked(userId);
        _fixture.GivenPasswordIsInvalid(userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldFailWith("Auth.InvalidCredentials");
    }

    [Fact]
    public async Task Handle_GivenLockedOutUser_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = AuthTestData.LoginCommand();
        var userDto = AuthTestData.User(id: userId, email: command.Email);

        _fixture.GivenUserFoundByEmail(userDto);
        _fixture.GivenUserIsLocked(userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldFailWith("Auth.AccountLocked");
    }
}
