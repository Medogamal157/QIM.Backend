using FluentValidation;

namespace QIM.Application.Features.Districts;

public class CreateDistrictCommandValidator : AbstractValidator<CreateDistrictCommand>
{
    public CreateDistrictCommandValidator()
    {
        RuleFor(x => x.Data.NameAr).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Data.NameEn).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Data.CityId).GreaterThan(0);
    }
}

public class UpdateDistrictCommandValidator : AbstractValidator<UpdateDistrictCommand>
{
    public UpdateDistrictCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Data.NameAr).MaximumLength(100).When(x => x.Data.NameAr != null);
        RuleFor(x => x.Data.NameEn).MaximumLength(100).When(x => x.Data.NameEn != null);
    }
}
