using FluentValidation;

namespace QIM.Application.Features.PlatformSettings;

public class UpdatePlatformSettingCommandValidator : AbstractValidator<UpdatePlatformSettingCommand>
{
    public UpdatePlatformSettingCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Data.Value).NotEmpty().MaximumLength(2000);
    }
}
