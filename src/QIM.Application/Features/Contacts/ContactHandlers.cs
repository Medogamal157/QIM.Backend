using AutoMapper;
using MediatR;
using QIM.Application.DTOs.Business;
using QIM.Application.Interfaces;
using QIM.Domain.Common.Enums;
using QIM.Domain.Entities;
using QIM.Shared.Models;

namespace QIM.Application.Features.Contacts;

// ══════════════════════════════════════════════
// ── Contact Request Handlers ──
// ══════════════════════════════════════════════

// ── Get All (Admin) ──
public record GetAllContactRequestsQuery(int Page = 1, int PageSize = 10, ContactStatus? Status = null)
    : IRequest<PaginatedResult<ContactRequestDto>>;

public class GetAllContactRequestsHandler : IRequestHandler<GetAllContactRequestsQuery, PaginatedResult<ContactRequestDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetAllContactRequestsHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<ContactRequestDto>> Handle(GetAllContactRequestsQuery request, CancellationToken ct)
    {
        var paged = await _uow.ContactRequests.GetPagedAsync(
            request.Page, request.PageSize,
            predicate: request.Status.HasValue ? c => c.Status == request.Status.Value : null,
            orderBy: c => c.CreatedAt,
            descending: true);

        var dtos = _mapper.Map<List<ContactRequestDto>>(paged.Items);
        return PaginatedResult<ContactRequestDto>.Success(dtos, paged.TotalCount, request.Page, request.PageSize);
    }
}

public record GetContactRequestByIdQuery(int Id) : IRequest<Result<ContactRequestDto>>;

public class GetContactRequestByIdHandler : IRequestHandler<GetContactRequestByIdQuery, Result<ContactRequestDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetContactRequestByIdHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<ContactRequestDto>> Handle(GetContactRequestByIdQuery request, CancellationToken ct)
    {
        var entity = await _uow.ContactRequests.GetByIdAsync(request.Id);
        if (entity is null)
            return Result<ContactRequestDto>.Failure($"ContactRequest with Id {request.Id} was not found.");

        return Result<ContactRequestDto>.Success(_mapper.Map<ContactRequestDto>(entity));
    }
}

// ── Create Contact Request (Public) ──
public record CreateContactRequestCommand(CreateContactRequest Data) : IRequest<Result<ContactRequestDto>>;

public class CreateContactRequestHandler : IRequestHandler<CreateContactRequestCommand, Result<ContactRequestDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public CreateContactRequestHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<ContactRequestDto>> Handle(CreateContactRequestCommand request, CancellationToken ct)
    {
        var entity = _mapper.Map<ContactRequest>(request.Data);
        entity.Status = ContactStatus.New;

        await _uow.ContactRequests.AddAsync(entity);
        await _uow.SaveChangesAsync(ct);

        return Result<ContactRequestDto>.Success(_mapper.Map<ContactRequestDto>(entity));
    }
}

// ── Update Contact Status (Admin) ──
public record UpdateContactStatusCommand(int Id, ContactStatus Status, string? AdminNotes = null)
    : IRequest<Result<ContactRequestDto>>;

public class UpdateContactStatusHandler : IRequestHandler<UpdateContactStatusCommand, Result<ContactRequestDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public UpdateContactStatusHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<ContactRequestDto>> Handle(UpdateContactStatusCommand request, CancellationToken ct)
    {
        var entity = await _uow.ContactRequests.GetByIdAsync(request.Id);
        if (entity is null)
            return Result<ContactRequestDto>.Failure($"ContactRequest with Id {request.Id} was not found.");

        entity.Status = request.Status;
        if (request.AdminNotes is not null)
            entity.AdminNotes = request.AdminNotes;

        await _uow.SaveChangesAsync(ct);
        return Result<ContactRequestDto>.Success(_mapper.Map<ContactRequestDto>(entity));
    }
}

// ══════════════════════════════════════════════
// ── Suggestion Handlers ──
// ══════════════════════════════════════════════

public record GetAllSuggestionsQuery(int Page = 1, int PageSize = 10, SuggestionStatus? Status = null)
    : IRequest<PaginatedResult<SuggestionDto>>;

public class GetAllSuggestionsHandler : IRequestHandler<GetAllSuggestionsQuery, PaginatedResult<SuggestionDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetAllSuggestionsHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<SuggestionDto>> Handle(GetAllSuggestionsQuery request, CancellationToken ct)
    {
        var paged = await _uow.Suggestions.GetPagedAsync(
            request.Page, request.PageSize,
            predicate: request.Status.HasValue ? s => s.Status == request.Status.Value : null,
            orderBy: s => s.CreatedAt,
            descending: true);

        var dtos = _mapper.Map<List<SuggestionDto>>(paged.Items);
        return PaginatedResult<SuggestionDto>.Success(dtos, paged.TotalCount, request.Page, request.PageSize);
    }
}

public record GetSuggestionByIdQuery(int Id) : IRequest<Result<SuggestionDto>>;

public class GetSuggestionByIdHandler : IRequestHandler<GetSuggestionByIdQuery, Result<SuggestionDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetSuggestionByIdHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<SuggestionDto>> Handle(GetSuggestionByIdQuery request, CancellationToken ct)
    {
        var entity = await _uow.Suggestions.GetByIdAsync(request.Id);
        if (entity is null)
            return Result<SuggestionDto>.Failure($"Suggestion with Id {request.Id} was not found.");

        return Result<SuggestionDto>.Success(_mapper.Map<SuggestionDto>(entity));
    }
}

// ── Create Suggestion (Public) ──
public record CreateSuggestionCommand(CreateSuggestionRequest Data) : IRequest<Result<SuggestionDto>>;

public class CreateSuggestionHandler : IRequestHandler<CreateSuggestionCommand, Result<SuggestionDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public CreateSuggestionHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<SuggestionDto>> Handle(CreateSuggestionCommand request, CancellationToken ct)
    {
        var entity = _mapper.Map<Suggestion>(request.Data);
        entity.Status = SuggestionStatus.New;

        await _uow.Suggestions.AddAsync(entity);
        await _uow.SaveChangesAsync(ct);

        return Result<SuggestionDto>.Success(_mapper.Map<SuggestionDto>(entity));
    }
}

// ── Update Suggestion Status (Admin) ──
public record UpdateSuggestionStatusCommand(int Id, SuggestionStatus Status)
    : IRequest<Result<SuggestionDto>>;

public class UpdateSuggestionStatusHandler : IRequestHandler<UpdateSuggestionStatusCommand, Result<SuggestionDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public UpdateSuggestionStatusHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<SuggestionDto>> Handle(UpdateSuggestionStatusCommand request, CancellationToken ct)
    {
        var entity = await _uow.Suggestions.GetByIdAsync(request.Id);
        if (entity is null)
            return Result<SuggestionDto>.Failure($"Suggestion with Id {request.Id} was not found.");

        entity.Status = request.Status;
        await _uow.SaveChangesAsync(ct);
        return Result<SuggestionDto>.Success(_mapper.Map<SuggestionDto>(entity));
    }
}
