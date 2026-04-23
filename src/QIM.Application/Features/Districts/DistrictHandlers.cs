using AutoMapper;
using MediatR;
using QIM.Application.DTOs.Location;
using QIM.Application.Interfaces;
using QIM.Shared.Models;

namespace QIM.Application.Features.Districts;

// ── Queries ──

public record GetAllDistrictsQuery : IRequest<Result<List<DistrictDto>>>;

public class GetAllDistrictsHandler : IRequestHandler<GetAllDistrictsQuery, Result<List<DistrictDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetAllDistrictsHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<List<DistrictDto>>> Handle(GetAllDistrictsQuery request, CancellationToken ct)
    {
        var districts = await _uow.Districts.GetAllAsync();
        return Result<List<DistrictDto>>.Success(_mapper.Map<List<DistrictDto>>(districts));
    }
}

public record GetDistrictsByCityQuery(int CityId) : IRequest<Result<List<DistrictDto>>>;

public class GetDistrictsByCityHandler : IRequestHandler<GetDistrictsByCityQuery, Result<List<DistrictDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetDistrictsByCityHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<List<DistrictDto>>> Handle(GetDistrictsByCityQuery request, CancellationToken ct)
    {
        var districts = await _uow.Districts.GetAllAsync(d => d.CityId == request.CityId);
        return Result<List<DistrictDto>>.Success(_mapper.Map<List<DistrictDto>>(districts));
    }
}

// ── Public Queries (filtered by IsEnabled) ──

public record GetPublicDistrictsByCityQuery(int CityId, string? Search = null) : IRequest<Result<List<DistrictDto>>>;

public class GetPublicDistrictsByCityHandler : IRequestHandler<GetPublicDistrictsByCityQuery, Result<List<DistrictDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetPublicDistrictsByCityHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<List<DistrictDto>>> Handle(GetPublicDistrictsByCityQuery request, CancellationToken ct)
    {
        var s = request.Search?.Trim().ToLower();
        var districts = string.IsNullOrEmpty(s)
            ? await _uow.Districts.GetAllAsync(d => d.CityId == request.CityId && d.IsEnabled)
            : await _uow.Districts.GetAllAsync(d => d.CityId == request.CityId && d.IsEnabled &&
                (d.NameAr.ToLower().Contains(s) || d.NameEn.ToLower().Contains(s)));
        return Result<List<DistrictDto>>.Success(_mapper.Map<List<DistrictDto>>(districts));
    }
}

public record GetPublicAllDistrictsQuery(string? Search = null) : IRequest<Result<List<DistrictDto>>>;

public class GetPublicAllDistrictsHandler : IRequestHandler<GetPublicAllDistrictsQuery, Result<List<DistrictDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetPublicAllDistrictsHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<List<DistrictDto>>> Handle(GetPublicAllDistrictsQuery request, CancellationToken ct)
    {
        var s = request.Search?.Trim().ToLower();
        var districts = string.IsNullOrEmpty(s)
            ? await _uow.Districts.GetAllAsync(d => d.IsEnabled)
            : await _uow.Districts.GetAllAsync(d => d.IsEnabled &&
                (d.NameAr.ToLower().Contains(s) || d.NameEn.ToLower().Contains(s)));
        return Result<List<DistrictDto>>.Success(_mapper.Map<List<DistrictDto>>(districts));
    }
}

public record GetDistrictByIdQuery(int Id) : IRequest<Result<DistrictDto>>;

public class GetDistrictByIdHandler : IRequestHandler<GetDistrictByIdQuery, Result<DistrictDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetDistrictByIdHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<DistrictDto>> Handle(GetDistrictByIdQuery request, CancellationToken ct)
    {
        var district = await _uow.Districts.GetByIdAsync(request.Id);
        if (district is null)
            return Result<DistrictDto>.Failure($"District with Id {request.Id} was not found.");

        return Result<DistrictDto>.Success(_mapper.Map<DistrictDto>(district));
    }
}

// ── Commands ──

public record CreateDistrictCommand(CreateDistrictRequest Data) : IRequest<Result<DistrictDto>>;

public class CreateDistrictHandler : IRequestHandler<CreateDistrictCommand, Result<DistrictDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public CreateDistrictHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<DistrictDto>> Handle(CreateDistrictCommand request, CancellationToken ct)
    {
        var city = await _uow.Cities.GetByIdAsync(request.Data.CityId);
        if (city is null)
            return Result<DistrictDto>.Failure($"City with Id {request.Data.CityId} was not found.");

        var entity = _mapper.Map<Domain.Entities.District>(request.Data);
        await _uow.Districts.AddAsync(entity);
        await _uow.SaveChangesAsync(ct);

        return Result<DistrictDto>.Success(_mapper.Map<DistrictDto>(entity));
    }
}

public record UpdateDistrictCommand(int Id, UpdateDistrictRequest Data) : IRequest<Result<DistrictDto>>;

public class UpdateDistrictHandler : IRequestHandler<UpdateDistrictCommand, Result<DistrictDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public UpdateDistrictHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<DistrictDto>> Handle(UpdateDistrictCommand request, CancellationToken ct)
    {
        var entity = await _uow.Districts.GetByIdAsync(request.Id);
        if (entity is null)
            return Result<DistrictDto>.Failure($"District with Id {request.Id} was not found.");

        _mapper.Map(request.Data, entity);
        await _uow.SaveChangesAsync(ct);
        return Result<DistrictDto>.Success(_mapper.Map<DistrictDto>(entity));
    }
}

public record DeleteDistrictCommand(int Id) : IRequest<Result>;

public class DeleteDistrictHandler : IRequestHandler<DeleteDistrictCommand, Result>
{
    private readonly IUnitOfWork _uow;

    public DeleteDistrictHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(DeleteDistrictCommand request, CancellationToken ct)
    {
        var entity = await _uow.Districts.GetByIdAsync(request.Id);
        if (entity is null)
            return Result.Failure($"District with Id {request.Id} was not found.");

        _uow.Districts.SoftDelete(entity);
        await _uow.SaveChangesAsync(ct);
        return Result.Success("District deleted.");
    }
}

public record ToggleDistrictEnabledCommand(int Id) : IRequest<Result<DistrictDto>>;

public class ToggleDistrictEnabledHandler : IRequestHandler<ToggleDistrictEnabledCommand, Result<DistrictDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public ToggleDistrictEnabledHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<DistrictDto>> Handle(ToggleDistrictEnabledCommand request, CancellationToken ct)
    {
        var entity = await _uow.Districts.GetByIdAsync(request.Id);
        if (entity is null)
            return Result<DistrictDto>.Failure($"District with Id {request.Id} was not found.");

        entity.IsEnabled = !entity.IsEnabled;
        await _uow.SaveChangesAsync(ct);
        return Result<DistrictDto>.Success(_mapper.Map<DistrictDto>(entity));
    }
}
