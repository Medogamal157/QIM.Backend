using FluentValidation;

namespace QIM.Application.Features.Countries;

public class CreateCountryCommandValidator : AbstractValidator<CreateCountryCommand>
{
    public CreateCountryCommandValidator()
    {
        RuleFor(x => x.Data.NameAr).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Data.NameEn).NotEmpty().MaximumLength(100);
    }
}

public class UpdateCountryCommandValidator : AbstractValidator<UpdateCountryCommand>
{
    public UpdateCountryCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Data.NameAr).MaximumLength(100).When(x => x.Data.NameAr != null);
        RuleFor(x => x.Data.NameEn).MaximumLength(100).When(x => x.Data.NameEn != null);
    }
}

public class ReorderCountriesCommandValidator : AbstractValidator<ReorderCountriesCommand>
{
    public ReorderCountriesCommandValidator()
    {
        RuleFor(x => x.OrderedIds).NotEmpty();
    }
}
