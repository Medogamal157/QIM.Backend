using AutoMapper;
using MediatR;
using QIM.Application.DTOs.Location;
using QIM.Application.Interfaces;
using QIM.Shared.Models;

namespace QIM.Application.Features.Cities;

// ── Queries ──

public record GetAllCitiesQuery : IRequest<Result<List<CityDto>>>;

public class GetAllCitiesHandler : IRequestHandler<GetAllCitiesQuery, Result<List<CityDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetAllCitiesHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<List<CityDto>>> Handle(GetAllCitiesQuery request, CancellationToken ct)
    {
        var cities = await _uow.Cities.GetAllAsync();
        return Result<List<CityDto>>.Success(_mapper.Map<List<CityDto>>(cities));
    }
}

public record GetCitiesByCountryQuery(int CountryId) : IRequest<Result<List<CityDto>>>;

public class GetCitiesByCountryHandler : IRequestHandler<GetCitiesByCountryQuery, Result<List<CityDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetCitiesByCountryHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<List<CityDto>>> Handle(GetCitiesByCountryQuery request, CancellationToken ct)
    {
        var cities = await _uow.Cities.GetAllAsync(c => c.CountryId == request.CountryId);
        return Result<List<CityDto>>.Success(_mapper.Map<List<CityDto>>(cities));
    }
}

// ── Public Queries (filtered by IsEnabled) ──

public record GetPublicCitiesByCountryQuery(int CountryId, string? Search = null) : IRequest<Result<List<CityDto>>>;

public class GetPublicCitiesByCountryHandler : IRequestHandler<GetPublicCitiesByCountryQuery, Result<List<CityDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetPublicCitiesByCountryHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<List<CityDto>>> Handle(GetPublicCitiesByCountryQuery request, CancellationToken ct)
    {
        var s = request.Search?.Trim().ToLower();
        var cities = string.IsNullOrEmpty(s)
            ? await _uow.Cities.GetAllAsync(c => c.CountryId == request.CountryId && c.IsEnabled)
            : await _uow.Cities.GetAllAsync(c => c.CountryId == request.CountryId && c.IsEnabled &&
                (c.NameAr.ToLower().Contains(s) || c.NameEn.ToLower().Contains(s)));
        return Result<List<CityDto>>.Success(_mapper.Map<List<CityDto>>(cities));
    }
}

public record GetPublicAllCitiesQuery(string? Search = null) : IRequest<Result<List<CityDto>>>;

public class GetPublicAllCitiesHandler : IRequestHandler<GetPublicAllCitiesQuery, Result<List<CityDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetPublicAllCitiesHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<List<CityDto>>> Handle(GetPublicAllCitiesQuery request, CancellationToken ct)
    {
        var s = request.Search?.Trim().ToLower();
        var cities = string.IsNullOrEmpty(s)
            ? await _uow.Cities.GetAllAsync(c => c.IsEnabled)
            : await _uow.Cities.GetAllAsync(c => c.IsEnabled &&
                (c.NameAr.ToLower().Contains(s) || c.NameEn.ToLower().Contains(s)));
        return Result<List<CityDto>>.Success(_mapper.Map<List<CityDto>>(cities));
    }
}

public record GetCityByIdQuery(int Id) : IRequest<Result<CityDto>>;

public class GetCityByIdHandler : IRequestHandler<GetCityByIdQuery, Result<CityDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetCityByIdHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<CityDto>> Handle(GetCityByIdQuery request, CancellationToken ct)
    {
        var city = await _uow.Cities.GetByIdAsync(request.Id);
        if (city is null)
            return Result<CityDto>.Failure($"City with Id {request.Id} was not found.");

        return Result<CityDto>.Success(_mapper.Map<CityDto>(city));
    }
}

// ── Commands ──

public record CreateCityCommand(CreateCityRequest Data) : IRequest<Result<CityDto>>;

public class CreateCityHandler : IRequestHandler<CreateCityCommand, Result<CityDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public CreateCityHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<CityDto>> Handle(CreateCityCommand request, CancellationToken ct)
    {
        var country = await _uow.Countries.GetByIdAsync(request.Data.CountryId);
        if (country is null)
            return Result<CityDto>.Failure($"Country with Id {request.Data.CountryId} was not found.");

        var entity = _mapper.Map<Domain.Entities.City>(request.Data);
        await _uow.Cities.AddAsync(entity);
        await _uow.SaveChangesAsync(ct);

        return Result<CityDto>.Success(_mapper.Map<CityDto>(entity));
    }
}

public record UpdateCityCommand(int Id, UpdateCityRequest Data) : IRequest<Result<CityDto>>;

public class UpdateCityHandler : IRequestHandler<UpdateCityCommand, Result<CityDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public UpdateCityHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<CityDto>> Handle(UpdateCityCommand request, CancellationToken ct)
    {
        var entity = await _uow.Cities.GetByIdAsync(request.Id);
        if (entity is null)
            return Result<CityDto>.Failure($"City with Id {request.Id} was not found.");

        _mapper.Map(request.Data, entity);
        await _uow.SaveChangesAsync(ct);
        return Result<CityDto>.Success(_mapper.Map<CityDto>(entity));
    }
}

public record DeleteCityCommand(int Id) : IRequest<Result>;

public class DeleteCityHandler : IRequestHandler<DeleteCityCommand, Result>
{
    private readonly IUnitOfWork _uow;

    public DeleteCityHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(DeleteCityCommand request, CancellationToken ct)
    {
        var entity = await _uow.Cities.GetByIdAsync(request.Id);
        if (entity is null)
            return Result.Failure($"City with Id {request.Id} was not found.");

        _uow.Cities.SoftDelete(entity);
        await _uow.SaveChangesAsync(ct);
        return Result.Success("City deleted.");
    }
}

public record ToggleCityEnabledCommand(int Id) : IRequest<Result<CityDto>>;

public class ToggleCityEnabledHandler : IRequestHandler<ToggleCityEnabledCommand, Result<CityDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public ToggleCityEnabledHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<CityDto>> Handle(ToggleCityEnabledCommand request, CancellationToken ct)
    {
        var entity = await _uow.Cities.GetByIdAsync(request.Id);
        if (entity is null)
            return Result<CityDto>.Failure($"City with Id {request.Id} was not found.");

        entity.IsEnabled = !entity.IsEnabled;
        await _uow.SaveChangesAsync(ct);
        return Result<CityDto>.Success(_mapper.Map<CityDto>(entity));
    }
}
