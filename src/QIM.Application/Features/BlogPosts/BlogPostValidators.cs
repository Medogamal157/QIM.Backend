using FluentValidation;

namespace QIM.Application.Features.BlogPosts;

public class CreateBlogPostCommandValidator : AbstractValidator<CreateBlogPostCommand>
{
    public CreateBlogPostCommandValidator()
    {
        RuleFor(x => x.Data.TitleAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Data.TitleEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Data.ContentAr).NotEmpty();
        RuleFor(x => x.Data.ContentEn).NotEmpty();
        RuleFor(x => x.Data.Category).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AuthorId).NotEmpty();
    }
}

public class UpdateBlogPostCommandValidator : AbstractValidator<UpdateBlogPostCommand>
{
    public UpdateBlogPostCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Data.TitleAr).MaximumLength(200).When(x => x.Data.TitleAr != null);
        RuleFor(x => x.Data.TitleEn).MaximumLength(200).When(x => x.Data.TitleEn != null);
        RuleFor(x => x.Data.Category).MaximumLength(100).When(x => x.Data.Category != null);
    }
}
