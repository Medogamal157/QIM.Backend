using MediatR;
using Microsoft.AspNetCore.Mvc;
using QIM.Application.Features.Countries;
using QIM.Application.Features.Cities;
using QIM.Application.Features.Districts;

namespace QIM.Presentation.Endpoints;

[Route("api/public")]
public class PublicLocationsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public PublicLocationsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("countries")]
    [ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "search" })]
    public async Task<IActionResult> GetCountries([FromQuery] string? search)
        => FromResult(await _mediator.Send(new GetPublicCountriesQuery(search)));

    [HttpGet("cities")]
    [ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "countryId", "search" })]
    public async Task<IActionResult> GetCities([FromQuery] int? countryId, [FromQuery] string? search)
    {
        if (countryId.HasValue)
            return FromResult(await _mediator.Send(new GetPublicCitiesByCountryQuery(countryId.Value, search)));
        return FromResult(await _mediator.Send(new GetPublicAllCitiesQuery(search)));
    }

    [HttpGet("districts")]
    [ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "cityId", "search" })]
    public async Task<IActionResult> GetDistricts([FromQuery] int? cityId, [FromQuery] string? search)
    {
        if (cityId.HasValue)
            return FromResult(await _mediator.Send(new GetPublicDistrictsByCityQuery(cityId.Value, search)));
        return FromResult(await _mediator.Send(new GetPublicAllDistrictsQuery(search)));
    }
}
