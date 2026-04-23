using AutoMapper;
using MediatR;
using QIM.Application.DTOs.Content;
using QIM.Application.Interfaces;
using QIM.Domain.Entities;
using QIM.Shared.Models;

namespace QIM.Application.Features.Advertisements;

// ── Admin Queries ──

public record GetAllAdvertisementsQuery : IRequest<Result<List<AdvertisementDto>>>;

public class GetAllAdvertisementsHandler : IRequestHandler<GetAllAdvertisementsQuery, Result<List<AdvertisementDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetAllAdvertisementsHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<List<AdvertisementDto>>> Handle(GetAllAdvertisementsQuery request, CancellationToken ct)
    {
        var ads = await _uow.Advertisements.GetAllAsync();
        return Result<List<AdvertisementDto>>.Success(_mapper.Map<List<AdvertisementDto>>(ads));
    }
}

// ── Public Query (active + in date range) ──

public record GetActiveAdvertisementsQuery(string? Position = null) : IRequest<Result<List<AdvertisementDto>>>;

public class GetActiveAdvertisementsHandler : IRequestHandler<GetActiveAdvertisementsQuery, Result<List<AdvertisementDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetActiveAdvertisementsHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<List<AdvertisementDto>>> Handle(GetActiveAdvertisementsQuery request, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var ads = await _uow.Advertisements.GetAllAsync(a =>
            a.IsActive
            && (!a.StartDate.HasValue || a.StartDate.Value <= now)
            && (!a.EndDate.HasValue || a.EndDate.Value >= now)
            && (request.Position == null || a.Position == request.Position));

        return Result<List<AdvertisementDto>>.Success(_mapper.Map<List<AdvertisementDto>>(ads));
    }
}

// ── Commands ──

public record CreateAdvertisementCommand(CreateAdvertisementRequest Data) : IRequest<Result<AdvertisementDto>>;

public class CreateAdvertisementHandler : IRequestHandler<CreateAdvertisementCommand, Result<AdvertisementDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public CreateAdvertisementHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<AdvertisementDto>> Handle(CreateAdvertisementCommand request, CancellationToken ct)
    {
        var entity = _mapper.Map<Advertisement>(request.Data);
        await _uow.Advertisements.AddAsync(entity);
        await _uow.SaveChangesAsync(ct);
        return Result<AdvertisementDto>.Success(_mapper.Map<AdvertisementDto>(entity));
    }
}

public record UpdateAdvertisementCommand(int Id, UpdateAdvertisementRequest Data) : IRequest<Result<AdvertisementDto>>;

public class UpdateAdvertisementHandler : IRequestHandler<UpdateAdvertisementCommand, Result<AdvertisementDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public UpdateAdvertisementHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<AdvertisementDto>> Handle(UpdateAdvertisementCommand request, CancellationToken ct)
    {
        var entity = await _uow.Advertisements.GetByIdAsync(request.Id);
        if (entity is null)
            return Result<AdvertisementDto>.Failure($"Advertisement with Id {request.Id} was not found.");

        _mapper.Map(request.Data, entity);
        await _uow.SaveChangesAsync(ct);
        return Result<AdvertisementDto>.Success(_mapper.Map<AdvertisementDto>(entity));
    }
}

public record DeleteAdvertisementCommand(int Id) : IRequest<Result>;

public class DeleteAdvertisementHandler : IRequestHandler<DeleteAdvertisementCommand, Result>
{
    private readonly IUnitOfWork _uow;

    public DeleteAdvertisementHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(DeleteAdvertisementCommand request, CancellationToken ct)
    {
        var entity = await _uow.Advertisements.GetByIdAsync(request.Id);
        if (entity is null)
            return Result.Failure($"Advertisement with Id {request.Id} was not found.");

        _uow.Advertisements.SoftDelete(entity);
        await _uow.SaveChangesAsync(ct);
        return Result.Success("Advertisement deleted.");
    }
}
