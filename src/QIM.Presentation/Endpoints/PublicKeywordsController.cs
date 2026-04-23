using MediatR;
using Microsoft.AspNetCore.Mvc;
using QIM.Application.Features.Keywords;

namespace QIM.Presentation.Endpoints;

[Route("api/public")]
public class PublicKeywordsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public PublicKeywordsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("keywords/search")]
    [ResponseCache(Duration = 30, VaryByQueryKeys = new[] { "query", "limit" })]
    public async Task<IActionResult> SearchKeywords(
        [FromQuery] string query,
        [FromQuery] int limit = 20)
        => FromResult(await _mediator.Send(new SearchKeywordsQuery(query, limit)));
}
