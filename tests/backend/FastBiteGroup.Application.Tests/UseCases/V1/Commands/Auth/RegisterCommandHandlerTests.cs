using FastBiteGroup.Application.Abstractions.Authentication;
using FastBiteGroup.Application.UseCases.V1.Commands.Auth;
using FastBiteGroup.Contract.Abstractions.Outbox;
using FastBiteGroup.Contract.Services.V1.Auth.Commands;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FastBiteGroup.Application.Tests.UseCases.V1.Commands.Auth;

public class RegisterCommandHandlerTests
{
    private readonly IUserAuthService _userAuthServiceMock;
    private readonly IOtpService _otpServiceMock;
    private readonly IIntegrationOutboxStore _outboxStoreMock;
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _userAuthServiceMock = Substitute.For<IUserAuthService>();
        _otpServiceMock = Substitute.For<IOtpService>();
        _outboxStoreMock = Substitute.For<IIntegrationOutboxStore>();

        _handler = new RegisterCommandHandler(
            _userAuthServiceMock,
            _otpServiceMock,
            _outboxStoreMock);
    }

    [Fact]
    public async Task Handle_GivenValidRequest_ReturnsSuccessAndGeneratesOTP()
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
            EmailConfirmed: false,
            IsActive: false,
            LastSeenAt: null,
            Roles: new List<string> { "Customer" });

        // Email does not exist yet
        _userAuthServiceMock.FindByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns((UserDto?)null);

        // CreateUserAsync returns userDto
        _userAuthServiceMock.CreateUserAsync(
                command.Email, command.Password, command.FirstName, command.LastName,
                command.DayOfBirth, Arg.Any<CancellationToken>())
            .Returns((userDto, (string?)null));

        _userAuthServiceMock.GenerateEmailConfirmationTokenAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns("fake-magic-link-token");
        _otpServiceMock.GenerateOtpAsync("REGISTER", command.Email, TimeSpan.FromMinutes(10), Arg.Any<CancellationToken>())
            .Returns("123456");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Message.Should().Contain("successfully");

        await _otpServiceMock.Received(1).GenerateOtpAsync(
            "REGISTER",
            command.Email,
            TimeSpan.FromMinutes(10),
            Arg.Any<CancellationToken>());

        await _outboxStoreMock.Received(1).AddAsync(Arg.Any<IntegrationOutboxMessage>(), Arg.Any<CancellationToken>());
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
            EmailConfirmed: true,
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
