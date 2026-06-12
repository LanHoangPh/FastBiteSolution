using FastBiteGroup.Contract.Services.V1.Workspace.Commands;

namespace FastBiteGroup.Application.Validators.Workspace;

public class UpdateWorkspaceValidator : AbstractValidator<UpdateWorkspaceCommand>
{
    public UpdateWorkspaceValidator()
    {
        RuleFor(x => x.WorkspaceId).NotEmpty();
        RuleFor(x => x.WorkspaceName)
            .NotEmpty().WithMessage("Workspace name is required.")
            .MaximumLength(255).WithMessage("Workspace name must not exceed 255 characters.");
        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

        RuleFor(x => x.Privacy)
            .InclusiveBetween(1, 2).WithMessage("Workspace privacy is not valid.");
        RuleFor(x => x.WorkspaceAvatarUrl)
            .MaximumLength(2048).WithMessage("Workspace avatar URL must not exceed 2048 characters.");
    }
}

public class InviteWorkspaceMemberValidator : AbstractValidator<InviteWorkspaceMemberCommand>
{
    public InviteWorkspaceMemberValidator()
    {
        RuleFor(x => x.WorkspaceId).NotEmpty();
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is not valid.")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters.");
    }
}

public class AcceptWorkspaceInvitationValidator : AbstractValidator<AcceptWorkspaceInvitationCommand>
{
    public AcceptWorkspaceInvitationValidator()
    {
        RuleFor(x => x.InvitationId).GreaterThan(0);
    }
}

public class JoinWorkspaceValidator : AbstractValidator<JoinWorkspaceCommand>
{
    public JoinWorkspaceValidator()
    {
        RuleFor(x => x.InvitationCode)
            .NotEmpty().WithMessage("Invitation code is required.")
            .MaximumLength(50).WithMessage("Invitation code must not exceed 50 characters.");
    }
}

public class CreateWorkspaceInviteLinkValidator : AbstractValidator<CreateWorkspaceInviteLinkCommand>
{
    public CreateWorkspaceInviteLinkValidator()
    {
        RuleFor(x => x.WorkspaceId).NotEmpty();
        RuleFor(x => x.MaxUses)
            .GreaterThan(0)
            .When(x => x.MaxUses.HasValue);
        RuleFor(x => x.ExpiresAt)
            .Must(value => !value.HasValue || value > DateTimeOffset.UtcNow)
            .WithMessage("Expiration must be in the future.")
            .When(x => x.ExpiresAt.HasValue);
    }
}

public class ArchiveWorkspaceValidator : AbstractValidator<ArchiveWorkspaceCommand>
{
    public ArchiveWorkspaceValidator()
    {
        RuleFor(x => x.WorkspaceId).NotEmpty();
    }
}
