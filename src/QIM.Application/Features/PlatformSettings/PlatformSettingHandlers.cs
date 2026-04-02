using AutoMapper;
using MediatR;
using QIM.Application.DTOs.Content;
using QIM.Application.Interfaces;
using QIM.Shared.Models;

namespace QIM.Application.Features.PlatformSettings;

// ── Queries ──

public record GetAllPlatformSettingsQuery : IRequest<Result<List<PlatformSettingDto>>>;

public class GetAllPlatformSettingsHandler : IRequestHandler<GetAllPlatformSettingsQuery, Result<List<PlatformSettingDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetAllPlatformSettingsHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<List<PlatformSettingDto>>> Handle(GetAllPlatformSettingsQuery request, CancellationToken ct)
    {
        var settings = await _uow.PlatformSettings.GetAllAsync();
        return Result<List<PlatformSettingDto>>.Success(_mapper.Map<List<PlatformSettingDto>>(settings));
    }
}

public record GetPlatformSettingsByGroupQuery(string Group) : IRequest<Result<List<PlatformSettingDto>>>;

public class GetPlatformSettingsByGroupHandler : IRequestHandler<GetPlatformSettingsByGroupQuery, Result<List<PlatformSettingDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetPlatformSettingsByGroupHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<List<PlatformSettingDto>>> Handle(GetPlatformSettingsByGroupQuery request, CancellationToken ct)
    {
        var settings = await _uow.PlatformSettings.GetAllAsync(s => s.Group == request.Group);
        return Result<List<PlatformSettingDto>>.Success(_mapper.Map<List<PlatformSettingDto>>(settings));
    }
}

// ── Commands ──

public record UpdatePlatformSettingCommand(int Id, UpdatePlatformSettingRequest Data) : IRequest<Result<PlatformSettingDto>>;

public class UpdatePlatformSettingHandler : IRequestHandler<UpdatePlatformSettingCommand, Result<PlatformSettingDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public UpdatePlatformSettingHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<PlatformSettingDto>> Handle(UpdatePlatformSettingCommand request, CancellationToken ct)
    {
        var entity = await _uow.PlatformSettings.GetByIdAsync(request.Id);
        if (entity is null)
            return Result<PlatformSettingDto>.Failure($"PlatformSetting with Id {request.Id} was not found.");

        _mapper.Map(request.Data, entity);
        await _uow.SaveChangesAsync(ct);
        return Result<PlatformSettingDto>.Success(_mapper.Map<PlatformSettingDto>(entity));
    }
}
