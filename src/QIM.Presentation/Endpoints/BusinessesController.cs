using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QIM.Application.DTOs.Business;
using QIM.Application.Features.Businesses;

namespace QIM.Presentation.Endpoints;

// ══════════════════════════════════════════════
// ── Admin Business Controller ──
// ══════════════════════════════════════════════

[Route("api/admin/businesses")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminBusinessesController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public AdminBusinessesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Domain.Common.Enums.BusinessStatus? status = null)
        => FromResult(await _mediator.Send(new GetAllBusinessesQuery(page, pageSize, status)));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
        => FromResult(await _mediator.Send(new GetBusinessByIdQuery(id)));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBusinessRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return FromResult(await _mediator.Send(new UpdateBusinessCommand(id, request, userId)));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
        => FromResult(await _mediator.Send(new DeleteBusinessCommand(id)));

    [HttpPatch("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id)
        => FromResult(await _mediator.Send(new ApproveBusinessCommand(id)));

    [HttpPatch("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] RejectBusinessRequest? request = null)
        => FromResult(await _mediator.Send(new RejectBusinessCommand(id, request?.Reason)));
}

public class RejectBusinessRequest
{
    public string? Reason { get; set; }
}

// ══════════════════════════════════════════════
// ── Owner / Authenticated Business Controller ──
// ══════════════════════════════════════════════

[Route("api/businesses")]
[Authorize]
public class BusinessesController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public BusinessesController(IMediator mediator) => _mediator = mediator;

    [HttpGet("my")]
    public async Task<IActionResult> GetMyBusinesses([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return FromResult(await _mediator.Send(new GetBusinessesByOwnerQuery(userId, page, pageSize)));
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
        => FromResult(await _mediator.Send(new GetBusinessByIdQuery(id)));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBusinessRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return FromResult(await _mediator.Send(new CreateBusinessCommand(request, userId)));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBusinessRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return FromResult(await _mediator.Send(new UpdateBusinessCommand(id, request, userId)));
    }

    // ── Addresses ──

    [HttpPost("{businessId:int}/addresses")]
    public async Task<IActionResult> AddAddress(int businessId, [FromBody] CreateBusinessAddressRequest request)
        => FromResult(await _mediator.Send(new AddBusinessAddressCommand(businessId, request)));

    [HttpPut("addresses/{id:int}")]
    public async Task<IActionResult> UpdateAddress(int id, [FromBody] UpdateBusinessAddressRequest request)
        => FromResult(await _mediator.Send(new UpdateBusinessAddressCommand(id, request)));

    [HttpDelete("addresses/{id:int}")]
    public async Task<IActionResult> DeleteAddress(int id)
        => FromResult(await _mediator.Send(new DeleteBusinessAddressCommand(id)));

    // ── Work Hours ──

    [HttpPut("{businessId:int}/work-hours")]
    public async Task<IActionResult> SetWorkHours(int businessId, [FromBody] List<SetWorkHoursRequest> items)
        => FromResult(await _mediator.Send(new SetBusinessWorkHoursCommand(businessId, items)));

    [HttpGet("{businessId:int}/work-hours")]
    [AllowAnonymous]
    public async Task<IActionResult> GetWorkHours(int businessId)
        => FromResult(await _mediator.Send(new GetBusinessWorkHoursQuery(businessId)));

    // ── Images ──

    [HttpPost("{businessId:int}/images")]
    public async Task<IActionResult> AddImage(
        int businessId,
        [FromQuery] string imageUrl,
        [FromQuery] bool isCover = false,
        [FromQuery] int sortOrder = 0)
        => FromResult(await _mediator.Send(new AddBusinessImageCommand(businessId, imageUrl, isCover, sortOrder)));

    [HttpDelete("images/{id:int}")]
    public async Task<IActionResult> DeleteImage(int id)
        => FromResult(await _mediator.Send(new DeleteBusinessImageCommand(id)));

    // ── Renew / Close ──

    [HttpPost("{id:int}/renew")]
    public async Task<IActionResult> Renew(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return FromResult(await _mediator.Send(new RenewBusinessCommand(id, userId)));
    }

    [HttpPost("{id:int}/close")]
    public async Task<IActionResult> Close(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return FromResult(await _mediator.Send(new CloseBusinessCommand(id, userId)));
    }
}

// ══════════════════════════════════════════════
// ── Public Business Controller ──
// ══════════════════════════════════════════════

[Route("api/public/businesses")]
public class PublicBusinessesController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public PublicBusinessesController(IMediator mediator) => _mediator = mediator;

    [HttpGet("search")]
    [ResponseCache(Duration = 60, VaryByQueryKeys = new[] { "keyword", "activityId", "specialityId", "countryId", "cityId", "districtId", "minRating", "searchIn", "sortBy", "page", "pageSize" })]
    public async Task<IActionResult> Search(
        [FromQuery] string? keyword = null,
        [FromQuery] int? activityId = null,
        [FromQuery] int? specialityId = null,
        [FromQuery] int? countryId = null,
        [FromQuery] int? cityId = null,
        [FromQuery] int? districtId = null,
        [FromQuery] double? minRating = null,
        [FromQuery] int? minReviewCount = null,
        [FromQuery] Domain.Common.Enums.SearchIn searchIn = Domain.Common.Enums.SearchIn.All,
        [FromQuery] Domain.Common.Enums.SortBy sortBy = Domain.Common.Enums.SortBy.HighestRated,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
        => FromResult(await _mediator.Send(new SearchBusinessesQuery(keyword, activityId, specialityId, countryId, cityId, districtId, minRating, minReviewCount, searchIn, sortBy, page, pageSize)));

    [HttpGet("autocomplete")]
    [ResponseCache(Duration = 30, VaryByQueryKeys = new[] { "query", "limit", "searchIn" })]
    public async Task<IActionResult> Autocomplete(
        [FromQuery] string query,
        [FromQuery] int limit = 10,
        [FromQuery] Domain.Common.Enums.SearchIn searchIn = Domain.Common.Enums.SearchIn.All)
        => FromResult(await _mediator.Send(new AutocompleteBusinessesQuery(query, limit, searchIn)));

    [HttpGet("top")]
    [ResponseCache(Duration = 120)]
    public async Task<IActionResult> GetTop([FromQuery] int count = 10)
        => FromResult(await _mediator.Send(new GetTopBusinessesQuery(count)));

    [HttpGet("{id:int}")]
    [ResponseCache(Duration = 60)]
    public async Task<IActionResult> GetById(int id)
        => FromResult(await _mediator.Send(new GetBusinessByIdQuery(id)));

    [HttpGet("by-code/{code}")]
    [ResponseCache(Duration = 60)]
    public async Task<IActionResult> GetByAccountCode(string code)
        => FromResult(await _mediator.Send(new GetBusinessByAccountCodeQuery(code)));

    [HttpGet("{businessId:int}/work-hours")]
    [ResponseCache(Duration = 300)]
    public async Task<IActionResult> GetWorkHours(int businessId)
        => FromResult(await _mediator.Send(new GetBusinessWorkHoursQuery(businessId)));
}
