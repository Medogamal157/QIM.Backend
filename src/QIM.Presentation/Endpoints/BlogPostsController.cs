using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QIM.Application.DTOs.Content;
using QIM.Application.Features.BlogPosts;

namespace QIM.Presentation.Endpoints;

[Route("api/admin/blog-posts")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class BlogPostsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public BlogPostsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => FromResult(await _mediator.Send(new GetAllBlogPostsQuery(page, pageSize)));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
        => FromResult(await _mediator.Send(new GetBlogPostByIdQuery(id)));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBlogPostRequest request)
    {
        var authorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return FromResult(await _mediator.Send(new CreateBlogPostCommand(request, authorId)));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBlogPostRequest request)
        => FromResult(await _mediator.Send(new UpdateBlogPostCommand(id, request)));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
        => FromResult(await _mediator.Send(new DeleteBlogPostCommand(id)));

    [HttpPatch("{id:int}/toggle-publish")]
    public async Task<IActionResult> TogglePublish(int id)
        => FromResult(await _mediator.Send(new ToggleBlogPostPublishCommand(id)));
}
