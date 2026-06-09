using System;
using System.Threading;
using System.Threading.Tasks;
using FastBiteGroup.Application.Abstractions.Authentication;
using FastBiteGroup.Application.UseCases.V1.Commands.Auth;
using FastBiteGroup.Contract.Abstractions.Shared;
using FastBiteGroup.Contract.Services.V1.Auth;
using FastBiteGroup.Contract.Services.V1.Auth.Commands;
using FastBiteGroup.Domain.Abstractions.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

namespace FastBiteGroup.Application.Tests.UseCases.V1.Commands.Auth;

public class ResetPasswordCommandHandlerTests
{
    private readonly Mock<IUserAuthService> _userAuthServiceMock;
    private readonly Mock<IOtpService> _otpServiceMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly ResetPasswordCommandHandler _handler;

    public ResetPasswordCommandHandlerTests()
    {
        _userAuthServiceMock = new Mock<IUserAuthService>();
        _otpServiceMock = new Mock<IOtpService>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();

        _handler = new ResetPasswordCommandHandler(
            _userAuthServiceMock.Object,
            _otpServiceMock.Object,
            _refreshTokenRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenValidateOtpReturnsMaxAttemptsReached()
    {
        // Arrange
        var command = new ResetPasswordCommand("test@example.com", "123456", "NewPassword123!");
        var userDto = new UserDto(Guid.NewGuid(), "test@example.com", "test", "first", "last", "first last", null, null, true, true, null, new[] { "Customer" });
        _userAuthServiceMock.Setup(x => x.FindByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userDto);
        _otpServiceMock.Setup(x => x.ValidateOtpAsync("RESET_PWD", command.Email, command.Otp, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OtpValidationResult.MaxAttemptsReached);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.AccountBlocked");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenValidateOtpReturnsInvalidCodeOrExpired()
    {
        // Arrange
        var command = new ResetPasswordCommand("test@example.com", "123456", "NewPassword123!");
        var userDto = new UserDto(Guid.NewGuid(), "test@example.com", "test", "first", "last", "first last", null, null, true, true, null, new[] { "Customer" });
        _userAuthServiceMock.Setup(x => x.FindByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userDto);
        _otpServiceMock.Setup(x => x.ValidateOtpAsync("RESET_PWD", command.Email, command.Otp, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OtpValidationResult.InvalidCode);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.InvalidOtp");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenResetPasswordAsyncFails()
    {
        // Arrange
        var command = new ResetPasswordCommand("test@example.com", "123456", "NewPassword123!");
        var userDto = new UserDto(Guid.NewGuid(), "test@example.com", "test", "first", "last", "first last", null, null, true, true, null, new[] { "Customer" });
        _userAuthServiceMock.Setup(x => x.FindByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userDto);
        _otpServiceMock.Setup(x => x.ValidateOtpAsync("RESET_PWD", command.Email, command.Otp, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OtpValidationResult.Success);
        _userAuthServiceMock.Setup(x => x.ResetPasswordAsync(command.Email, command.NewPassword, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(AuthErrors.ResetFailed("Failed to reset password")));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.ResetFailed");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_AndRevokeTokens_WhenAllValid()
    {
        // Arrange
        var command = new ResetPasswordCommand("test@example.com", "123456", "NewPassword123!");
        var userDto = new UserDto(Guid.NewGuid(), "test@example.com", "test", "first", "last", "first last", null, null, true, true, null, new[] { "Customer" });
        _userAuthServiceMock.Setup(x => x.FindByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userDto);
        _otpServiceMock.Setup(x => x.ValidateOtpAsync("RESET_PWD", command.Email, command.Otp, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OtpValidationResult.Success);
        _userAuthServiceMock.Setup(x => x.ResetPasswordAsync(command.Email, command.NewPassword, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _refreshTokenRepositoryMock.Verify(x => x.RevokeAllForUserAsync(userDto.Id, It.IsAny<CancellationToken>()), Times.Once);
    }
}
