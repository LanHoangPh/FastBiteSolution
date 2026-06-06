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
        var command = new AuthCommands.RegisterCommand("test@test.com", "Password123!@", "First", "Last", new DateTime(1990, 1, 1));
        var userId = Guid.NewGuid();
        var userDto = new UserDto(
            Id: userId,
            Email: command.Email,
            UserName: command.Email,
            FirstName: command.FirstName,
            LastName: command.LastName,
            FullName: "First Last",
            AvatarUrl: null,
            Bio: null,
            IsActive: true,
            LastSeenAt: null,
            Roles: new List<string> { "Customer" });

        // Email does not exist yet
        _userAuthServiceMock.FindByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns((UserDto?)null);

        // CreateUserAsync now returns (UserDto?, string?) directly — no second FindByEmail
        _userAuthServiceMock.CreateUserAsync(
                command.Email, command.Password, command.FirstName, command.LastName,
                command.DayOfBirth, Arg.Any<CancellationToken>())
            .Returns((userDto, (string?)null));

        _jwtTokenServiceMock.GenerateAccessToken(userId, userDto.Email, userDto.UserName, userDto.FirstName, userDto.LastName, userDto.Roles)
            .Returns(("access-token", "jti-123", DateTime.UtcNow.AddMinutes(15)));
        _jwtTokenServiceMock.GenerateRefreshToken()
            .Returns("refresh-token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("access-token");
        result.Value.TokenType.Should().Be("Bearer");
        result.Value.User.IsActive.Should().BeTrue();
        _refreshTokenRepositoryMock.Received(1).Add(Arg.Is<AppRefreshToken>(r => r.UserId == userId && r.Token == "refresh-token"));

        // Ensure FindByEmailAsync is only called once (check), NOT twice (was a bug)
        await _userAuthServiceMock.Received(1).FindByEmailAsync(command.Email, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_GivenExistingEmail_ReturnsFailure()
    {
        // Arrange
        var command = new AuthCommands.RegisterCommand("test@test.com", "Password123!@", "First", "Last", new DateTime(1990, 1, 1));
        var existingUserDto = new UserDto(
            Id: Guid.NewGuid(),
            Email: command.Email,
            UserName: command.Email,
            FirstName: "First",
            LastName: "Last",
            FullName: "First Last",
            AvatarUrl: null,
            Bio: null,
            IsActive: true,
            LastSeenAt: null,
            Roles: new List<string> { "Customer" });

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
        var command = new AuthCommands.RegisterCommand("test@test.com", "Password123!@", "First", "Last", new DateTime(1990, 1, 1));

        _userAuthServiceMock.FindByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns((UserDto?)null);

        // CreateUserAsync returns null UserDto when it fails
        _userAuthServiceMock.CreateUserAsync(command.Email, command.Password, command.FirstName, command.LastName, command.DayOfBirth, Arg.Any<CancellationToken>())
            .Returns(((UserDto?)null, "Password requires upper case."));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.RegistrationFailed");
        result.Error.Message.Should().Be("Password requires upper case.");
    }
}
