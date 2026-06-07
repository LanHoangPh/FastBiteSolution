using FastBiteGroup.Application.Abstractions.Authentication;
using FastBiteGroup.Application.UseCases.V1.Commands.Auth;
using FastBiteGroup.Contract.Services.V1.Auth.Commands;
using FastBiteGroup.Domain.Abstractions.Repositories;
using FastBiteGroup.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FastBiteGroup.Application.Tests.UseCases.V1.Commands.Auth;

public class VerifyEmailCommandHandlerTests
{
    private readonly Mock<IUserAuthService> _userAuthServiceMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<ILogger<VerifyEmailCommandHandler>> _loggerMock;

    private readonly VerifyEmailCommandHandler _handler;

    public VerifyEmailCommandHandlerTests()
    {
        _userAuthServiceMock = new Mock<IUserAuthService>();
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _loggerMock = new Mock<ILogger<VerifyEmailCommandHandler>>();

        _handler = new VerifyEmailCommandHandler(
            _userAuthServiceMock.Object,
            _jwtTokenServiceMock.Object,
            _refreshTokenRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new AuthCommands.VerifyEmailCommand("test@example.com", "email-confirmation-token");
        _userAuthServiceMock.Setup(x => x.FindByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDto?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.UserNotFound");
    }

    [Fact]
    public async Task Handle_WhenEmailAlreadyConfirmed_ShouldReturnFailure()
    {
        // Arrange
        var command = new AuthCommands.VerifyEmailCommand("test@example.com", "email-confirmation-token");
        var user = new UserDto(Guid.NewGuid(), command.Email, "testuser", "Test", "User", "Test User", null, null, true, true, null, new List<string>());

        _userAuthServiceMock.Setup(x => x.FindByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.EmailAlreadyConfirmed");
    }

    [Fact]
    public async Task Handle_WhenTokenConfirmationFails_ShouldReturnFailure()
    {
        // Arrange
        var command = new AuthCommands.VerifyEmailCommand("test@example.com", "invalid-token");
        var user = new UserDto(Guid.NewGuid(), command.Email, "testuser", "Test", "User", "Test User", null, null, false, true, null, new List<string>());

        _userAuthServiceMock.Setup(x => x.FindByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userAuthServiceMock.Setup(x => x.ConfirmEmailWithTokenAsync(command.Email, command.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.InvalidToken");
    }

    [Fact]
    public async Task Handle_WhenSuccess_ShouldReturnAuthResponse()
    {
        // Arrange
        var command = new AuthCommands.VerifyEmailCommand("test@example.com", "email-confirmation-token");
        var user = new UserDto(Guid.NewGuid(), command.Email, "testuser", "Test", "User", "Test User", null, null, false, true, null, new List<string>());
        var verifiedUser = new UserDto(user.Id, command.Email, "testuser", "Test", "User", "Test User", null, null, true, true, null, new List<string>());

        var accessToken = "access-token";
        var jti = "jti-value";
        var accessExpiresAt = DateTime.UtcNow.AddMinutes(15);
        var refreshToken = "refresh-token";

        // Initial fetch then refetch after successful validation
        _userAuthServiceMock.SetupSequence(x => x.FindByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user)
            .ReturnsAsync(verifiedUser);

        _userAuthServiceMock.Setup(x => x.ConfirmEmailWithTokenAsync(command.Email, command.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _jwtTokenServiceMock.Setup(x => x.GenerateAccessToken(user.Id, user.Email, user.UserName, user.FirstName, user.LastName, user.Roles))
            .Returns((accessToken, jti, accessExpiresAt));

        _jwtTokenServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns(refreshToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be(accessToken);
        result.Value.RefreshToken.Should().Be(refreshToken);

        _refreshTokenRepositoryMock.Verify(x => x.Add(It.Is<AppRefreshToken>(r => r.Token == refreshToken && r.UserId == user.Id)), Times.Once);
    }
}
