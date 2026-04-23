using FluentValidation;

namespace QIM.Application.Features.Specialities;

public class CreateSpecialityCommandValidator : AbstractValidator<CreateSpecialityCommand>
{
    public CreateSpecialityCommandValidator()
    {
        RuleFor(x => x.Data.NameAr).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Data.NameEn).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Data.ActivityId).GreaterThan(0);
    }
}

public class UpdateSpecialityCommandValidator : AbstractValidator<UpdateSpecialityCommand>
{
    public UpdateSpecialityCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Data.NameAr).MaximumLength(100).When(x => x.Data.NameAr != null);
        RuleFor(x => x.Data.NameEn).MaximumLength(100).When(x => x.Data.NameEn != null);
    }
}
