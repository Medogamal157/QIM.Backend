using FluentValidation;

namespace QIM.Application.Features.Cities;

public class CreateCityCommandValidator : AbstractValidator<CreateCityCommand>
{
    public CreateCityCommandValidator()
    {
        RuleFor(x => x.Data.NameAr).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Data.NameEn).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Data.CountryId).GreaterThan(0);
    }
}

public class UpdateCityCommandValidator : AbstractValidator<UpdateCityCommand>
{
    public UpdateCityCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Data.NameAr).MaximumLength(100).When(x => x.Data.NameAr != null);
        RuleFor(x => x.Data.NameEn).MaximumLength(100).When(x => x.Data.NameEn != null);
    }
}
