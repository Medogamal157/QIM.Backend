using FluentValidation;

namespace QIM.Application.Features.Activities;

public class CreateActivityCommandValidator : AbstractValidator<CreateActivityCommand>
{
    public CreateActivityCommandValidator()
    {
        RuleFor(x => x.Data.NameAr).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Data.NameEn).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Data.DescriptionAr).MaximumLength(500).When(x => x.Data.DescriptionAr != null);
        RuleFor(x => x.Data.DescriptionEn).MaximumLength(500).When(x => x.Data.DescriptionEn != null);
        RuleFor(x => x.Data.IconUrl).MaximumLength(500).When(x => x.Data.IconUrl != null);
        RuleFor(x => x.Data.Color).MaximumLength(20).When(x => x.Data.Color != null);
    }
}

public class UpdateActivityCommandValidator : AbstractValidator<UpdateActivityCommand>
{
    public UpdateActivityCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Data.NameAr).MaximumLength(100).When(x => x.Data.NameAr != null);
        RuleFor(x => x.Data.NameEn).MaximumLength(100).When(x => x.Data.NameEn != null);
        RuleFor(x => x.Data.DescriptionAr).MaximumLength(500).When(x => x.Data.DescriptionAr != null);
        RuleFor(x => x.Data.DescriptionEn).MaximumLength(500).When(x => x.Data.DescriptionEn != null);
    }
}
