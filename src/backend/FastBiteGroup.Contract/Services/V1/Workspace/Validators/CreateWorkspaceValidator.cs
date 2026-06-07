using FastBiteGroup.Contract.Services.V1.Workspace.Commands;
using FluentValidation;

namespace FastBiteGroup.Contract.Services.V1.Workspace.Validators;

public class CreateWorkspaceValidator : AbstractValidator<CreateWorkspaceCommand>
{
    public CreateWorkspaceValidator()
    {
        RuleFor(x => x.WorkspaceName)
            .NotEmpty().WithMessage("Tên Workspace không được để trống.")
            .MaximumLength(255).WithMessage("Tên Workspace tối đa 255 ký tự.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Mô tả tối đa 1000 ký tự.");
            
        RuleFor(x => x.WorkspaceType)
            .IsInEnum().WithMessage("Loại Workspace không hợp lệ.");

        RuleFor(x => x.Privacy)
            .IsInEnum().WithMessage("Quyền riêng tư không hợp lệ.");
    }
}
