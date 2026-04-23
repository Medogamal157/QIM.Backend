using AutoMapper;
using MediatR;
using QIM.Application.DTOs.Activity;
using QIM.Application.Interfaces;
using QIM.Shared.Models;

namespace QIM.Application.Features.Specialities;

// ── Queries ──

public record GetAllSpecialitiesQuery : IRequest<Result<List<SpecialityDto>>>;

public class GetAllSpecialitiesHandler : IRequestHandler<GetAllSpecialitiesQuery, Result<List<SpecialityDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetAllSpecialitiesHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<List<SpecialityDto>>> Handle(GetAllSpecialitiesQuery request, CancellationToken ct)
    {
        var specialities = await _uow.Specialities.GetAllAsync();
        return Result<List<SpecialityDto>>.Success(_mapper.Map<List<SpecialityDto>>(specialities));
    }
}

public record GetSpecialitiesByActivityQuery(int ActivityId) : IRequest<Result<List<SpecialityDto>>>;

public class GetSpecialitiesByActivityHandler : IRequestHandler<GetSpecialitiesByActivityQuery, Result<List<SpecialityDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetSpecialitiesByActivityHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<List<SpecialityDto>>> Handle(GetSpecialitiesByActivityQuery request, CancellationToken ct)
    {
        var specialities = await _uow.Specialities.GetAllAsync(s => s.ActivityId == request.ActivityId);
        return Result<List<SpecialityDto>>.Success(_mapper.Map<List<SpecialityDto>>(specialities));
    }
}

// ── Public Queries (filtered by IsEnabled) ──

public record GetPublicSpecialitiesByActivityQuery(int ActivityId, string? Search = null) : IRequest<Result<List<SpecialityDto>>>;

public class GetPublicSpecialitiesByActivityHandler : IRequestHandler<GetPublicSpecialitiesByActivityQuery, Result<List<SpecialityDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetPublicSpecialitiesByActivityHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<List<SpecialityDto>>> Handle(GetPublicSpecialitiesByActivityQuery request, CancellationToken ct)
    {
        var s = request.Search?.Trim().ToLower();
        var specialities = string.IsNullOrEmpty(s)
            ? await _uow.Specialities.GetAllAsync(st => st.ActivityId == request.ActivityId && st.IsEnabled)
            : await _uow.Specialities.GetAllAsync(st => st.ActivityId == request.ActivityId && st.IsEnabled &&
                (st.NameAr.ToLower().Contains(s) || st.NameEn.ToLower().Contains(s)));
        return Result<List<SpecialityDto>>.Success(_mapper.Map<List<SpecialityDto>>(specialities));
    }
}

public record GetPublicAllSpecialitiesQuery(string? Search = null) : IRequest<Result<List<SpecialityDto>>>;

public class GetPublicAllSpecialitiesHandler : IRequestHandler<GetPublicAllSpecialitiesQuery, Result<List<SpecialityDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetPublicAllSpecialitiesHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<List<SpecialityDto>>> Handle(GetPublicAllSpecialitiesQuery request, CancellationToken ct)
    {
        var s = request.Search?.Trim().ToLower();
        var specialities = string.IsNullOrEmpty(s)
            ? await _uow.Specialities.GetAllAsync(st => st.IsEnabled)
            : await _uow.Specialities.GetAllAsync(st => st.IsEnabled &&
                (st.NameAr.ToLower().Contains(s) || st.NameEn.ToLower().Contains(s)));
        return Result<List<SpecialityDto>>.Success(_mapper.Map<List<SpecialityDto>>(specialities));
    }
}

public record GetSpecialityByIdQuery(int Id) : IRequest<Result<SpecialityDto>>;

public class GetSpecialityByIdHandler : IRequestHandler<GetSpecialityByIdQuery, Result<SpecialityDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetSpecialityByIdHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<SpecialityDto>> Handle(GetSpecialityByIdQuery request, CancellationToken ct)
    {
        var speciality = await _uow.Specialities.GetByIdAsync(request.Id);
        if (speciality is null)
            return Result<SpecialityDto>.Failure($"Speciality with Id {request.Id} was not found.");

        return Result<SpecialityDto>.Success(_mapper.Map<SpecialityDto>(speciality));
    }
}

// ── Commands ──

public record CreateSpecialityCommand(CreateSpecialityRequest Data) : IRequest<Result<SpecialityDto>>;

public class CreateSpecialityHandler : IRequestHandler<CreateSpecialityCommand, Result<SpecialityDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public CreateSpecialityHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<SpecialityDto>> Handle(CreateSpecialityCommand request, CancellationToken ct)
    {
        var activity = await _uow.Activities.GetByIdAsync(request.Data.ActivityId);
        if (activity is null)
            return Result<SpecialityDto>.Failure($"Activity with Id {request.Data.ActivityId} was not found.");

        var entity = _mapper.Map<Domain.Entities.Speciality>(request.Data);
        await _uow.Specialities.AddAsync(entity);
        await _uow.SaveChangesAsync(ct);

        return Result<SpecialityDto>.Success(_mapper.Map<SpecialityDto>(entity));
    }
}

public record UpdateSpecialityCommand(int Id, UpdateSpecialityRequest Data) : IRequest<Result<SpecialityDto>>;

public class UpdateSpecialityHandler : IRequestHandler<UpdateSpecialityCommand, Result<SpecialityDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public UpdateSpecialityHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<SpecialityDto>> Handle(UpdateSpecialityCommand request, CancellationToken ct)
    {
        var entity = await _uow.Specialities.GetByIdAsync(request.Id);
        if (entity is null)
            return Result<SpecialityDto>.Failure($"Speciality with Id {request.Id} was not found.");

        _mapper.Map(request.Data, entity);
        await _uow.SaveChangesAsync(ct);
        return Result<SpecialityDto>.Success(_mapper.Map<SpecialityDto>(entity));
    }
}

public record DeleteSpecialityCommand(int Id) : IRequest<Result>;

public class DeleteSpecialityHandler : IRequestHandler<DeleteSpecialityCommand, Result>
{
    private readonly IUnitOfWork _uow;

    public DeleteSpecialityHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(DeleteSpecialityCommand request, CancellationToken ct)
    {
        var entity = await _uow.Specialities.GetByIdAsync(request.Id);
        if (entity is null)
            return Result.Failure($"Speciality with Id {request.Id} was not found.");

        _uow.Specialities.SoftDelete(entity);
        await _uow.SaveChangesAsync(ct);
        return Result.Success("Speciality deleted.");
    }
}

public record ToggleSpecialityEnabledCommand(int Id) : IRequest<Result<SpecialityDto>>;

public class ToggleSpecialityEnabledHandler : IRequestHandler<ToggleSpecialityEnabledCommand, Result<SpecialityDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public ToggleSpecialityEnabledHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<SpecialityDto>> Handle(ToggleSpecialityEnabledCommand request, CancellationToken ct)
    {
        var entity = await _uow.Specialities.GetByIdAsync(request.Id);
        if (entity is null)
            return Result<SpecialityDto>.Failure($"Speciality with Id {request.Id} was not found.");

        entity.IsEnabled = !entity.IsEnabled;
        await _uow.SaveChangesAsync(ct);
        return Result<SpecialityDto>.Success(_mapper.Map<SpecialityDto>(entity));
    }
}
