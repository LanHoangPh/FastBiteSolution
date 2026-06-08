using FastBiteGroup.Application.Validators;
using FastBiteGroup.Contract.Services.V1.Workspace.Commands;
using FluentAssertions;

namespace FastBiteGroup.Contract.Tests.Workspace;

public class WorkspaceValidatorTests
{
    [Fact]
    public void CreateWorkspaceValidator_WhenNameEmpty_ShouldFail()
    {
        var validator = new CreateWorkspaceValidator();
        var command = new CreateWorkspaceCommand(string.Empty, null, 1, 1, null);

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateWorkspaceCommand.WorkspaceName));
    }

    [Fact]
    public void CreateWorkspaceValidator_WhenEnumValuesInvalid_ShouldFail()
    {
        var validator = new CreateWorkspaceValidator();
        var command = new CreateWorkspaceCommand("Acme", null, 99, 99, null);

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateWorkspaceCommand.WorkspaceType));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateWorkspaceCommand.Privacy));
    }

    [Fact]
    public void InviteWorkspaceMemberValidator_WhenEmailInvalid_ShouldFail()
    {
        var validator = new InviteWorkspaceMemberValidator();
        var command = new InviteWorkspaceMemberCommand(Guid.NewGuid(), "not-an-email");

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(InviteWorkspaceMemberCommand.Email));
    }

    [Fact]
    public void JoinWorkspaceValidator_WhenCodeEmpty_ShouldFail()
    {
        var validator = new JoinWorkspaceValidator();
        var command = new JoinWorkspaceCommand(string.Empty);

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(JoinWorkspaceCommand.InvitationCode));
    }
}
