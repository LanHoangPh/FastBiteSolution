using FastBiteGroup.Application.Tests.Common.Builders;
using FastBiteGroup.Application.Validators.Auth;
using FastBiteGroup.Contract.Services.V1.Auth.Commands;
using FluentAssertions;

namespace FastBiteGroup.Application.Tests.Validators.Auth;

public class AuthCommandValidatorTests
{
    private readonly LoginCommandValidator _loginValidator = new();
    private readonly RegisterCommandValidator _registerValidator = new();

    [Fact]
    public void LoginCommand_WithValidData_ShouldPass()
    {
        var result = _loginValidator.Validate(AuthTestData.LoginCommand());

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(InvalidLoginCommands))]
    public void LoginCommand_WithInvalidData_ShouldFail(AuthCommands.LoginCommand command, string propertyName)
    {
        var result = _loginValidator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == propertyName);
    }

    [Fact]
    public void RegisterCommand_WithValidData_ShouldPass()
    {
        var result = _registerValidator.Validate(AuthTestData.RegisterCommand());

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(InvalidRegisterCommands))]
    public void RegisterCommand_WithInvalidData_ShouldFail(AuthCommands.RegisterCommand command, string propertyName)
    {
        var result = _registerValidator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == propertyName);
    }

    public static TheoryData<AuthCommands.LoginCommand, string> InvalidLoginCommands()
        => new()
        {
            { AuthTestData.LoginCommand(email: ""), nameof(AuthCommands.LoginCommand.Email) },
            { AuthTestData.LoginCommand(email: "invalid-email"), nameof(AuthCommands.LoginCommand.Email) },
            { AuthTestData.LoginCommand(password: ""), nameof(AuthCommands.LoginCommand.Password) },
            { AuthTestData.LoginCommand(password: "short"), nameof(AuthCommands.LoginCommand.Password) }
        };

    public static TheoryData<AuthCommands.RegisterCommand, string> InvalidRegisterCommands()
        => new()
        {
            { AuthTestData.RegisterCommand(email: ""), nameof(AuthCommands.RegisterCommand.Email) },
            { AuthTestData.RegisterCommand(email: "invalid-email"), nameof(AuthCommands.RegisterCommand.Email) },
            { AuthTestData.RegisterCommand(email: new string('a', 257) + "@test.com"), nameof(AuthCommands.RegisterCommand.Email) },
            { AuthTestData.RegisterCommand(password: "short"), nameof(AuthCommands.RegisterCommand.Password) },
            { AuthTestData.RegisterCommand(password: "password123"), nameof(AuthCommands.RegisterCommand.Password) },
            { AuthTestData.RegisterCommand(password: "Password"), nameof(AuthCommands.RegisterCommand.Password) },
            { AuthTestData.RegisterCommand(firstName: ""), nameof(AuthCommands.RegisterCommand.FirstName) },
            { AuthTestData.RegisterCommand(firstName: new string('A', 101)), nameof(AuthCommands.RegisterCommand.FirstName) },
            { AuthTestData.RegisterCommand(lastName: ""), nameof(AuthCommands.RegisterCommand.LastName) },
            { AuthTestData.RegisterCommand(lastName: new string('A', 101)), nameof(AuthCommands.RegisterCommand.LastName) },
            { AuthTestData.RegisterCommand(dayOfBirth: DateTime.UtcNow.AddDays(1)), nameof(AuthCommands.RegisterCommand.DayOfBirth) },
            { AuthTestData.RegisterCommand(dayOfBirth: DateTime.UtcNow.AddYears(-121)), nameof(AuthCommands.RegisterCommand.DayOfBirth) }
        };
}
