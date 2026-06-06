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
    private readonly Mock<IOtpService> _otpServiceMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<ILogger<VerifyEmailCommandHandler>> _loggerMock;

    private readonly VerifyEmailCommandHandler _handler;

    public VerifyEmailCommandHandlerTests()
    {
        _userAuthServiceMock = new Mock<IUserAuthService>();
        _otpServiceMock = new Mock<IOtpService>();
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _loggerMock = new Mock<ILogger<VerifyEmailCommandHandler>>();

        _handler = new VerifyEmailCommandHandler(
            _userAuthServiceMock.Object,
            _otpServiceMock.Object,
            _jwtTokenServiceMock.Object,
            _refreshTokenRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new AuthCommands.VerifyEmailCommand("test@example.com", "123456");
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
        var command = new AuthCommands.VerifyEmailCommand("test@example.com", "123456");
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
    public async Task Handle_WhenOtpLimitReached_ShouldReturnFailure()
    {
        // Arrange
        var command = new AuthCommands.VerifyEmailCommand("test@example.com", "123456");
        var user = new UserDto(Guid.NewGuid(), command.Email, "testuser", "Test", "User", "Test User", null, null, false, true, null, new List<string>());
        
        _userAuthServiceMock.Setup(x => x.FindByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _otpServiceMock.Setup(x => x.ValidateOtpAsync("REGISTER", command.Email, command.Code, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OtpValidationResult.MaxAttemptsReached);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.OtpLimitReached");
    }

    [Fact]
    public async Task Handle_WhenBothOtpAndMagicLinkFail_ShouldReturnFailure()
    {
        // Arrange
        var command = new AuthCommands.VerifyEmailCommand("test@example.com", "invalid-code");
        var user = new UserDto(Guid.NewGuid(), command.Email, "testuser", "Test", "User", "Test User", null, null, false, true, null, new List<string>());
        
        _userAuthServiceMock.Setup(x => x.FindByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _otpServiceMock.Setup(x => x.ValidateOtpAsync("REGISTER", command.Email, command.Code, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OtpValidationResult.InvalidCode);

        _userAuthServiceMock.Setup(x => x.ConfirmEmailWithTokenAsync(command.Email, command.Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.InvalidCode");
    }

    [Fact]
    public async Task Handle_WhenSuccess_ShouldReturnAuthResponse()
    {
        // Arrange
        var command = new AuthCommands.VerifyEmailCommand("test@example.com", "123456");
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

        _otpServiceMock.Setup(x => x.ValidateOtpAsync("REGISTER", command.Email, command.Code, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OtpValidationResult.Success);

        _userAuthServiceMock.Setup(x => x.ActivateUserAsync(command.Email, It.IsAny<CancellationToken>()))
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
