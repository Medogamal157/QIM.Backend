using MediatR;
using Microsoft.AspNetCore.Mvc;
using QIM.Application.Features.PlatformSettings;
using QIM.Application.Features.BlogPosts;
using QIM.Application.Features.Advertisements;

namespace QIM.Presentation.Endpoints;

[Route("api/public")]
public class PublicContentController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public PublicContentController(IMediator mediator) => _mediator = mediator;

    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings()
        => FromResult(await _mediator.Send(new GetAllPlatformSettingsQuery()));

    [HttpGet("blog-posts")]
    [ResponseCache(Duration = 300)]
    public async Task<IActionResult> GetBlogPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => FromResult(await _mediator.Send(new GetPublishedBlogPostsQuery(page, pageSize)));

    [HttpGet("blog-posts/{id:int}")]
    [ResponseCache(Duration = 60)]
    public async Task<IActionResult> GetBlogPost(int id)
        => FromResult(await _mediator.Send(new GetBlogPostByIdQuery(id)));

    [HttpGet("advertisements")]
    [ResponseCache(Duration = 300)]
    public async Task<IActionResult> GetActiveAds([FromQuery] string? position = null)
        => FromResult(await _mediator.Send(new GetActiveAdvertisementsQuery(position)));
}
