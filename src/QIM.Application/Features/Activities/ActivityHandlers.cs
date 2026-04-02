using AutoMapper;
using MediatR;
using QIM.Application.DTOs.Activity;
using QIM.Application.Interfaces;
using QIM.Shared.Models;

namespace QIM.Application.Features.Activities;

// ── Queries ──

public record GetAllActivitiesQuery : IRequest<Result<List<ActivityDto>>>;

public class GetAllActivitiesHandler : IRequestHandler<GetAllActivitiesQuery, Result<List<ActivityDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetAllActivitiesHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<List<ActivityDto>>> Handle(GetAllActivitiesQuery request, CancellationToken ct)
    {
        var activities = await _uow.Activities.GetAllOrderedAsync(c => c.SortOrder);
        return Result<List<ActivityDto>>.Success(_mapper.Map<List<ActivityDto>>(activities));
    }
}

public record GetActivityTreeQuery : IRequest<Result<List<ActivityDto>>>;

public class GetActivityTreeHandler : IRequestHandler<GetActivityTreeQuery, Result<List<ActivityDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetActivityTreeHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<List<ActivityDto>>> Handle(GetActivityTreeQuery request, CancellationToken ct)
    {
        // Get root activities with SubActivities included
        var rootActivities = await _uow.Activities.GetAllAsync(
            c => c.ParentActivityId == null,
            query => query.OrderBy(c => c.SortOrder),
            c => c.SubActivities);

        return Result<List<ActivityDto>>.Success(_mapper.Map<List<ActivityDto>>(rootActivities));
    }
}

public record GetActivityByIdQuery(int Id) : IRequest<Result<ActivityDto>>;

public class GetActivityByIdHandler : IRequestHandler<GetActivityByIdQuery, Result<ActivityDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetActivityByIdHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<ActivityDto>> Handle(GetActivityByIdQuery request, CancellationToken ct)
    {
        var activity = await _uow.Activities.GetByIdAsync(request.Id);
        if (activity is null)
            return Result<ActivityDto>.Failure($"Activity with Id {request.Id} was not found.");

        return Result<ActivityDto>.Success(_mapper.Map<ActivityDto>(activity));
    }
}

// ── Public Queries (filtered by IsEnabled) ──

public record GetPublicActivityTreeQuery(string? Search = null) : IRequest<Result<List<ActivityDto>>>;

public class GetPublicActivityTreeHandler : IRequestHandler<GetPublicActivityTreeQuery, Result<List<ActivityDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetPublicActivityTreeHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<List<ActivityDto>>> Handle(GetPublicActivityTreeQuery request, CancellationToken ct)
    {
        var rootActivities = await _uow.Activities.GetAllAsync(
            c => c.ParentActivityId == null && c.IsEnabled,
            query => query.OrderBy(c => c.SortOrder),
            c => c.SubActivities);

        // Filter sub-activities by IsEnabled
        foreach (var act in rootActivities)
            act.SubActivities = act.SubActivities.Where(sc => sc.IsEnabled).OrderBy(sc => sc.SortOrder).ToList();

        var s = request.Search?.Trim().ToLower();
        if (!string.IsNullOrEmpty(s))
        {
            rootActivities = rootActivities
                .Where(c => c.NameAr.ToLower().Contains(s) || c.NameEn.ToLower().Contains(s)
                    || c.SubActivities.Any(sc => sc.NameAr.ToLower().Contains(s) || sc.NameEn.ToLower().Contains(s)))
                .ToList();
        }

        return Result<List<ActivityDto>>.Success(_mapper.Map<List<ActivityDto>>(rootActivities));
    }
}

// ── Commands ──

public record CreateActivityCommand(CreateActivityRequest Data) : IRequest<Result<ActivityDto>>;

public class CreateActivityHandler : IRequestHandler<CreateActivityCommand, Result<ActivityDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public CreateActivityHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<ActivityDto>> Handle(CreateActivityCommand request, CancellationToken ct)
    {
        if (request.Data.ParentActivityId.HasValue)
        {
            var parent = await _uow.Activities.GetByIdAsync(request.Data.ParentActivityId.Value);
            if (parent is null)
                return Result<ActivityDto>.Failure($"Parent activity with Id {request.Data.ParentActivityId.Value} was not found.");
        }

        var entity = _mapper.Map<Domain.Entities.Activity>(request.Data);
        await _uow.Activities.AddAsync(entity);
        await _uow.SaveChangesAsync(ct);

        return Result<ActivityDto>.Success(_mapper.Map<ActivityDto>(entity));
    }
}

public record UpdateActivityCommand(int Id, UpdateActivityRequest Data) : IRequest<Result<ActivityDto>>;

public class UpdateActivityHandler : IRequestHandler<UpdateActivityCommand, Result<ActivityDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public UpdateActivityHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<ActivityDto>> Handle(UpdateActivityCommand request, CancellationToken ct)
    {
        var entity = await _uow.Activities.GetByIdAsync(request.Id);
        if (entity is null)
            return Result<ActivityDto>.Failure($"Activity with Id {request.Id} was not found.");

        _mapper.Map(request.Data, entity);
        await _uow.SaveChangesAsync(ct);
        return Result<ActivityDto>.Success(_mapper.Map<ActivityDto>(entity));
    }
}

public record DeleteActivityCommand(int Id) : IRequest<Result>;

public class DeleteActivityHandler : IRequestHandler<DeleteActivityCommand, Result>
{
    private readonly IUnitOfWork _uow;

    public DeleteActivityHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(DeleteActivityCommand request, CancellationToken ct)
    {
        var entity = await _uow.Activities.GetByIdAsync(request.Id);
        if (entity is null)
            return Result.Failure($"Activity with Id {request.Id} was not found.");

        _uow.Activities.SoftDelete(entity);
        await _uow.SaveChangesAsync(ct);
        return Result.Success("Activity deleted.");
    }
}
