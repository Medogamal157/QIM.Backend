using AutoMapper;
using MediatR;
using QIM.Application.DTOs.Business;
using QIM.Application.Interfaces;
using QIM.Domain.Common.Enums;
using QIM.Domain.Entities;
using QIM.Shared.Models;

namespace QIM.Application.Features.BusinessClaims;

// ── Queries ──

public record GetAllClaimsQuery(int Page = 1, int PageSize = 10, ClaimStatus? Status = null)
    : IRequest<PaginatedResult<BusinessClaimDto>>;

public class GetAllClaimsHandler : IRequestHandler<GetAllClaimsQuery, PaginatedResult<BusinessClaimDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetAllClaimsHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<BusinessClaimDto>> Handle(GetAllClaimsQuery request, CancellationToken ct)
    {
        var paged = await _uow.BusinessClaims.GetPagedAsync(
            request.Page, request.PageSize,
            predicate: request.Status.HasValue ? c => c.Status == request.Status.Value : null,
            orderBy: c => c.CreatedAt,
            descending: true,
            includes: [c => c.Business, c => c.User]);

        var dtos = _mapper.Map<List<BusinessClaimDto>>(paged.Items);
        return PaginatedResult<BusinessClaimDto>.Success(dtos, paged.TotalCount, request.Page, request.PageSize);
    }
}

public record GetClaimByIdQuery(int Id) : IRequest<Result<BusinessClaimDto>>;

public class GetClaimByIdHandler : IRequestHandler<GetClaimByIdQuery, Result<BusinessClaimDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetClaimByIdHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<BusinessClaimDto>> Handle(GetClaimByIdQuery request, CancellationToken ct)
    {
        var entity = await _uow.BusinessClaims.FirstOrDefaultAsync(
            c => c.Id == request.Id,
            c => c.Business, c => c.User);

        if (entity is null)
            return Result<BusinessClaimDto>.Failure($"BusinessClaim with Id {request.Id} was not found.");

        return Result<BusinessClaimDto>.Success(_mapper.Map<BusinessClaimDto>(entity));
    }
}

// ── Commands ──

public record CreateBusinessClaimCommand(CreateBusinessClaimRequest Data, string UserId)
    : IRequest<Result<BusinessClaimDto>>;

public class CreateBusinessClaimHandler : IRequestHandler<CreateBusinessClaimCommand, Result<BusinessClaimDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public CreateBusinessClaimHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<BusinessClaimDto>> Handle(CreateBusinessClaimCommand request, CancellationToken ct)
    {
        var biz = await _uow.Businesses.GetByIdAsync(request.Data.BusinessId);
        if (biz is null)
            return Result<BusinessClaimDto>.Failure($"Business with Id {request.Data.BusinessId} was not found.");

        // Check for existing pending claim from same user
        var existing = await _uow.BusinessClaims.AnyAsync(
            c => c.BusinessId == request.Data.BusinessId
                 && c.UserId == request.UserId
                 && c.Status == ClaimStatus.Pending);
        if (existing)
            return Result<BusinessClaimDto>.Failure("You already have a pending claim for this business.");

        var entity = _mapper.Map<BusinessClaim>(request.Data);
        entity.UserId = request.UserId;
        entity.Status = ClaimStatus.Pending;

        await _uow.BusinessClaims.AddAsync(entity);
        await _uow.SaveChangesAsync(ct);

        return Result<BusinessClaimDto>.Success(_mapper.Map<BusinessClaimDto>(entity));
    }
}

public record ApproveClaimCommand(int Id) : IRequest<Result<BusinessClaimDto>>;

public class ApproveClaimHandler : IRequestHandler<ApproveClaimCommand, Result<BusinessClaimDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public ApproveClaimHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<BusinessClaimDto>> Handle(ApproveClaimCommand request, CancellationToken ct)
    {
        var entity = await _uow.BusinessClaims.FirstOrDefaultAsync(
            c => c.Id == request.Id,
            c => c.Business, c => c.User);

        if (entity is null)
            return Result<BusinessClaimDto>.Failure($"BusinessClaim with Id {request.Id} was not found.");

        entity.Status = ClaimStatus.Approved;

        // Transfer ownership
        var biz = await _uow.Businesses.GetByIdAsync(entity.BusinessId);
        if (biz is not null)
        {
            biz.OwnerId = entity.UserId;
            biz.IsVerified = true;
        }

        await _uow.SaveChangesAsync(ct);
        return Result<BusinessClaimDto>.Success(_mapper.Map<BusinessClaimDto>(entity));
    }
}

public record RejectClaimCommand(int Id) : IRequest<Result<BusinessClaimDto>>;

public class RejectClaimHandler : IRequestHandler<RejectClaimCommand, Result<BusinessClaimDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public RejectClaimHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<BusinessClaimDto>> Handle(RejectClaimCommand request, CancellationToken ct)
    {
        var entity = await _uow.BusinessClaims.GetByIdAsync(request.Id);
        if (entity is null)
            return Result<BusinessClaimDto>.Failure($"BusinessClaim with Id {request.Id} was not found.");

        entity.Status = ClaimStatus.Rejected;
        await _uow.SaveChangesAsync(ct);
        return Result<BusinessClaimDto>.Success(_mapper.Map<BusinessClaimDto>(entity));
    }
}
