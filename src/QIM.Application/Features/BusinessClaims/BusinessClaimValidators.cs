using FluentValidation;
using QIM.Application.DTOs.Business;

namespace QIM.Application.Features.BusinessClaims;

public class CreateBusinessClaimRequestValidator : AbstractValidator<CreateBusinessClaimRequest>
{
    public CreateBusinessClaimRequestValidator()
    {
        RuleFor(x => x.BusinessId).GreaterThan(0);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
    }
}
