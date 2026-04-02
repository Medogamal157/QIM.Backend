using AutoMapper;
using MediatR;
using QIM.Application.DTOs.Location;
using QIM.Application.Interfaces;
using QIM.Shared.Models;

namespace QIM.Application.Features.Countries;

// ── Queries ──

public record GetAllCountriesQuery : IRequest<Result<List<CountryDto>>>;

public class GetAllCountriesHandler : IRequestHandler<GetAllCountriesQuery, Result<List<CountryDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetAllCountriesHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<List<CountryDto>>> Handle(GetAllCountriesQuery request, CancellationToken ct)
    {
        var countries = await _uow.Countries.GetAllOrderedAsync(c => c.SortOrder);
        return Result<List<CountryDto>>.Success(_mapper.Map<List<CountryDto>>(countries));
    }
}

public record GetCountryByIdQuery(int Id) : IRequest<Result<CountryDto>>;

public class GetCountryByIdHandler : IRequestHandler<GetCountryByIdQuery, Result<CountryDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetCountryByIdHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<CountryDto>> Handle(GetCountryByIdQuery request, CancellationToken ct)
    {
        var country = await _uow.Countries.GetByIdAsync(request.Id);
        if (country is null)
            return Result<CountryDto>.Failure($"Country with Id {request.Id} was not found.");

        return Result<CountryDto>.Success(_mapper.Map<CountryDto>(country));
    }
}

// ── Public Query (filtered by IsEnabled) ──

public record GetPublicCountriesQuery(string? Search = null) : IRequest<Result<List<CountryDto>>>;

public class GetPublicCountriesHandler : IRequestHandler<GetPublicCountriesQuery, Result<List<CountryDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetPublicCountriesHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<List<CountryDto>>> Handle(GetPublicCountriesQuery request, CancellationToken ct)
    {
        var s = request.Search?.Trim().ToLower();
        var countries = string.IsNullOrEmpty(s)
            ? await _uow.Countries.GetAllAsync(c => c.IsEnabled)
            : await _uow.Countries.GetAllAsync(c => c.IsEnabled &&
                (c.NameAr.ToLower().Contains(s) || c.NameEn.ToLower().Contains(s)));
        return Result<List<CountryDto>>.Success(_mapper.Map<List<CountryDto>>(countries));
    }
}

// ── Commands ──

public record CreateCountryCommand(CreateCountryRequest Data) : IRequest<Result<CountryDto>>;

public class CreateCountryHandler : IRequestHandler<CreateCountryCommand, Result<CountryDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public CreateCountryHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<CountryDto>> Handle(CreateCountryCommand request, CancellationToken ct)
    {
        var entity = _mapper.Map<Domain.Entities.Country>(request.Data);

        if (request.Data.IsDefault)
        {
            // Clear other defaults
            var allCountries = await _uow.Countries.GetAllAsync(c => c.IsDefault);
            foreach (var c in allCountries) c.IsDefault = false;
        }

        await _uow.Countries.AddAsync(entity);
        await _uow.SaveChangesAsync(ct);

        return Result<CountryDto>.Success(_mapper.Map<CountryDto>(entity));
    }
}

public record UpdateCountryCommand(int Id, UpdateCountryRequest Data) : IRequest<Result<CountryDto>>;

public class UpdateCountryHandler : IRequestHandler<UpdateCountryCommand, Result<CountryDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public UpdateCountryHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<CountryDto>> Handle(UpdateCountryCommand request, CancellationToken ct)
    {
        var entity = await _uow.Countries.GetByIdAsync(request.Id);
        if (entity is null)
            return Result<CountryDto>.Failure($"Country with Id {request.Id} was not found.");

        _mapper.Map(request.Data, entity);

        if (request.Data.IsDefault)
        {
            var others = await _uow.Countries.GetAllAsync(c => c.IsDefault && c.Id != request.Id);
            foreach (var c in others) c.IsDefault = false;
        }

        await _uow.SaveChangesAsync(ct);
        return Result<CountryDto>.Success(_mapper.Map<CountryDto>(entity));
    }
}

public record DeleteCountryCommand(int Id) : IRequest<Result>;

public class DeleteCountryHandler : IRequestHandler<DeleteCountryCommand, Result>
{
    private readonly IUnitOfWork _uow;

    public DeleteCountryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(DeleteCountryCommand request, CancellationToken ct)
    {
        var entity = await _uow.Countries.GetByIdAsync(request.Id);
        if (entity is null)
            return Result.Failure($"Country with Id {request.Id} was not found.");

        _uow.Countries.SoftDelete(entity);
        await _uow.SaveChangesAsync(ct);
        return Result.Success("Country deleted.");
    }
}

public record ToggleCountryEnabledCommand(int Id) : IRequest<Result<CountryDto>>;

public class ToggleCountryEnabledHandler : IRequestHandler<ToggleCountryEnabledCommand, Result<CountryDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public ToggleCountryEnabledHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<CountryDto>> Handle(ToggleCountryEnabledCommand request, CancellationToken ct)
    {
        var entity = await _uow.Countries.GetByIdAsync(request.Id);
        if (entity is null)
            return Result<CountryDto>.Failure($"Country with Id {request.Id} was not found.");

        entity.IsEnabled = !entity.IsEnabled;
        await _uow.SaveChangesAsync(ct);
        return Result<CountryDto>.Success(_mapper.Map<CountryDto>(entity));
    }
}

public record SetDefaultCountryCommand(int Id) : IRequest<Result<CountryDto>>;

public class SetDefaultCountryHandler : IRequestHandler<SetDefaultCountryCommand, Result<CountryDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public SetDefaultCountryHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<CountryDto>> Handle(SetDefaultCountryCommand request, CancellationToken ct)
    {
        var entity = await _uow.Countries.GetByIdAsync(request.Id);
        if (entity is null)
            return Result<CountryDto>.Failure($"Country with Id {request.Id} was not found.");

        var others = await _uow.Countries.GetAllAsync(c => c.IsDefault);
        foreach (var c in others) c.IsDefault = false;

        entity.IsDefault = true;
        await _uow.SaveChangesAsync(ct);
        return Result<CountryDto>.Success(_mapper.Map<CountryDto>(entity));
    }
}

public record ReorderCountriesCommand(List<int> OrderedIds) : IRequest<Result>;

public class ReorderCountriesHandler : IRequestHandler<ReorderCountriesCommand, Result>
{
    private readonly IUnitOfWork _uow;

    public ReorderCountriesHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(ReorderCountriesCommand request, CancellationToken ct)
    {
        for (int i = 0; i < request.OrderedIds.Count; i++)
        {
            var entity = await _uow.Countries.GetByIdAsync(request.OrderedIds[i]);
            if (entity is not null)
                entity.SortOrder = i;
        }

        await _uow.SaveChangesAsync(ct);
        return Result.Success("Countries reordered.");
    }
}
