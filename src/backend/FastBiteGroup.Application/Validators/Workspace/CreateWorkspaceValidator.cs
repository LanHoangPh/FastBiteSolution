using FastBiteGroup.Contract.Services.V1.Workspace.Commands;
using FluentValidation;

namespace FastBiteGroup.Application.Validators;

public class CreateWorkspaceValidator : AbstractValidator<CreateWorkspaceCommand>
{
    public CreateWorkspaceValidator()
    {
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
