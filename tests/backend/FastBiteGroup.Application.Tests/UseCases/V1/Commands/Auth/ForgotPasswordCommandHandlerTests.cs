using System;
using System.Threading;
using System.Threading.Tasks;
using FastBiteGroup.Application.Abstractions.Authentication;
using FastBiteGroup.Application.Abstractions.Caching;
using FastBiteGroup.Application.UseCases.V1.Commands.Auth;
using FastBiteGroup.Contract.Abstractions.Outbox;
using FastBiteGroup.Contract.Abstractions.Shared;
using FastBiteGroup.Contract.Services.V1.Auth.Commands;
using FastBiteGroup.Contract.Services.V1.Auth.Events;
using FluentAssertions;
using Moq;
using Xunit;

namespace FastBiteGroup.Application.Tests.UseCases.V1.Commands.Auth;

public class ForgotPasswordCommandHandlerTests
{
    private readonly Mock<IUserAuthService> _userAuthServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IOtpService> _otpServiceMock;
    private readonly Mock<IIntegrationOutboxStore> _outboxStoreMock;
    private readonly ForgotPasswordCommandHandler _handler;

    public ForgotPasswordCommandHandlerTests()
    {
        _userAuthServiceMock = new Mock<IUserAuthService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _otpServiceMock = new Mock<IOtpService>();
        _outboxStoreMock = new Mock<IIntegrationOutboxStore>();

        _handler = new ForgotPasswordCommandHandler(
            _userAuthServiceMock.Object,
            _cacheServiceMock.Object,
            _otpServiceMock.Object,
            _outboxStoreMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenUserDoesNotExist()
    {
        // Arrange
        var command = new ForgotPasswordCommand("test@example.com");
        _userAuthServiceMock.Setup(x => x.FindByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDto?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _cacheServiceMock.Verify(x => x.GetAsync<int>(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _otpServiceMock.Verify(x => x.GenerateOtpAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
        _outboxStoreMock.Verify(x => x.AddAsync(It.IsAny<IntegrationOutboxMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCacheCountIsGreaterOrEqual3()
    {
        // Arrange
        var command = new ForgotPasswordCommand("test@example.com");
        var userDto = new UserDto(Guid.NewGuid(), "test@example.com", "test", "first", "last", "first last", null, null, true, true, null, new[] { "Customer" });
        _userAuthServiceMock.Setup(x => x.FindByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userDto);
        _cacheServiceMock.Setup(x => x.GetAsync<int>($"OTP_RESET_COUNT_{userDto.Email}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.TooManyRequests");
        _otpServiceMock.Verify(x => x.GenerateOtpAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenAllValid()
    {
        // Arrange
        var command = new ForgotPasswordCommand("test@example.com");
        var userDto = new UserDto(Guid.NewGuid(), "test@example.com", "test", "first", "last", "first last", null, null, true, true, null, new[] { "Customer" });
        _userAuthServiceMock.Setup(x => x.FindByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userDto);
        _cacheServiceMock.Setup(x => x.GetAsync<int>($"OTP_RESET_COUNT_{userDto.Email}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        _otpServiceMock.Setup(x => x.GenerateOtpAsync("RESET_PWD", userDto.Email, TimeSpan.FromMinutes(10), It.IsAny<CancellationToken>()))
            .ReturnsAsync("123456");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _outboxStoreMock.Verify(x => x.AddAsync(It.IsAny<IntegrationOutboxMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
