using FastBiteGroup.Application.Abstractions.Authentication;
using FastBiteGroup.Application.UseCases.V1.Commands.Auth;
using FastBiteGroup.Contract.Services.V1.Auth.Commands;
using FastBiteGroup.Domain.Abstractions.Repositories;
using FastBiteGroup.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FastBiteGroup.Application.Tests.UseCases.V1.Commands.Auth;

public class RegisterCommandHandlerTests
{
    private readonly IUserAuthService _userAuthServiceMock;
    private readonly IJwtTokenService _jwtTokenServiceMock;
    private readonly IRefreshTokenRepository _refreshTokenRepositoryMock;
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _userAuthServiceMock = Substitute.For<IUserAuthService>();
        _jwtTokenServiceMock = Substitute.For<IJwtTokenService>();
        _refreshTokenRepositoryMock = Substitute.For<IRefreshTokenRepository>();

        _handler = new RegisterCommandHandler(
            _userAuthServiceMock,
            _jwtTokenServiceMock,
            _refreshTokenRepositoryMock);
    }

    [Fact]
    public async Task Handle_GivenValidRequest_ReturnsSuccessWithTokens()
    {
        // Arrange
        var command = new AuthCommands.RegisterCommand("test@test.com", "Password123!", "First", "Last", new DateTime(1990, 1, 1));
        var userId = Guid.NewGuid();
        var userDto = new UserDto(userId, command.Email, command.Email, command.FirstName, command.LastName, new List<string> { "Customer" });

        // Email does not exist yet
        _userAuthServiceMock.FindByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns((UserDto?)null, userDto); // Return null on first call (check), return userDto on second call (fetch after create)

        _userAuthServiceMock.CreateUserAsync(command.Email, command.Password, command.FirstName, command.LastName, command.DayOfBirth, Arg.Any<CancellationToken>())
            .Returns((true, null));

        _jwtTokenServiceMock.GenerateAccessToken(userId, userDto.Email, userDto.UserName, userDto.FirstName, userDto.LastName, userDto.Roles)
            .Returns(("access-token", "jti-123", DateTime.UtcNow.AddMinutes(15)));
        _jwtTokenServiceMock.GenerateRefreshToken()
            .Returns("refresh-token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("access-token");
        _refreshTokenRepositoryMock.Received(1).Add(Arg.Is<AppRefreshToken>(r => r.UserId == userId && r.Token == "refresh-token"));
    }

    [Fact]
    public async Task Handle_GivenExistingEmail_ReturnsFailure()
    {
        // Arrange
        var command = new AuthCommands.RegisterCommand("test@test.com", "Password123!", "First", "Last", new DateTime(1990, 1, 1));
        var existingUserDto = new UserDto(Guid.NewGuid(), command.Email, command.Email, "First", "Last", new List<string> { "Customer" });

        _userAuthServiceMock.FindByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(existingUserDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.EmailAlreadyExists");
        await _userAuthServiceMock.DidNotReceiveWithAnyArgs().CreateUserAsync(default!, default!, default!, default!, default!);
    }

    [Fact]
    public async Task Handle_GivenCreateFails_ReturnsFailure()
    {
        // Arrange
        var command = new AuthCommands.RegisterCommand("test@test.com", "Password123!", "First", "Last", new DateTime(1990, 1, 1));
        
        _userAuthServiceMock.FindByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns((UserDto?)null);

        _userAuthServiceMock.CreateUserAsync(command.Email, command.Password, command.FirstName, command.LastName, command.DayOfBirth, Arg.Any<CancellationToken>())
            .Returns((false, "Password requires upper case."));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.RegistrationFailed");
        result.Error.Message.Should().Be("Password requires upper case.");
    }
}
