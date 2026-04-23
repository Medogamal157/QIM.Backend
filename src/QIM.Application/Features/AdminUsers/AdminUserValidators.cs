using FluentValidation;

namespace QIM.Application.Features.AdminUsers;

public class CreateAdminUserCommandValidator : AbstractValidator<CreateAdminUserCommand>
{
    public CreateAdminUserCommandValidator()
    {
        RuleFor(x => x.Data.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Data.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Data.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.Data.Role).NotEmpty().Must(r => r == "Admin" || r == "SuperAdmin")
            .WithMessage("Role must be Admin or SuperAdmin.");
    }
}

public class UpdateAdminUserCommandValidator : AbstractValidator<UpdateAdminUserCommand>
{
    public UpdateAdminUserCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Data.FullName).MaximumLength(200).When(x => x.Data.FullName != null);
        RuleFor(x => x.Data.Email).EmailAddress().When(x => x.Data.Email != null);
    }
}

public class ChangeAdminRoleCommandValidator : AbstractValidator<ChangeAdminRoleCommand>
{
    public ChangeAdminRoleCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Data.Role).NotEmpty().Must(r => r == "Admin" || r == "SuperAdmin")
            .WithMessage("Role must be Admin or SuperAdmin.");
    }
}
