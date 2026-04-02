using AutoMapper;
using MediatR;
using QIM.Application.DTOs.Business;
using QIM.Application.Interfaces;
using QIM.Domain.Common.Enums;
using QIM.Domain.Entities;
using QIM.Shared.Models;

namespace QIM.Application.Features.Reviews;

// ══════════════════════════════════════════════
// ── Queries ──
// ══════════════════════════════════════════════

// ── Get reviews for a business (public) ──
public record GetBusinessReviewsQuery(int BusinessId, int Page = 1, int PageSize = 10)
    : IRequest<PaginatedResult<ReviewDto>>;

public class GetBusinessReviewsHandler : IRequestHandler<GetBusinessReviewsQuery, PaginatedResult<ReviewDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetBusinessReviewsHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<ReviewDto>> Handle(GetBusinessReviewsQuery request, CancellationToken ct)
    {
        var paged = await _uow.Reviews.GetPagedAsync(
            request.Page, request.PageSize,
            predicate: r => r.BusinessId == request.BusinessId && r.Status == ReviewStatus.Approved,
            orderBy: r => r.CreatedAt,
            descending: true,
            includes: [r => r.User]);

        var dtos = _mapper.Map<List<ReviewDto>>(paged.Items);
        return PaginatedResult<ReviewDto>.Success(dtos, paged.TotalCount, request.Page, request.PageSize);
    }
}

// ── Get all reviews (admin) ──
public record GetAllReviewsQuery(int Page = 1, int PageSize = 10, ReviewStatus? Status = null)
    : IRequest<PaginatedResult<ReviewDto>>;

public class GetAllReviewsHandler : IRequestHandler<GetAllReviewsQuery, PaginatedResult<ReviewDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetAllReviewsHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<ReviewDto>> Handle(GetAllReviewsQuery request, CancellationToken ct)
    {
        var paged = await _uow.Reviews.GetPagedAsync(
            request.Page, request.PageSize,
            predicate: request.Status.HasValue ? r => r.Status == request.Status.Value : null,
            orderBy: r => r.CreatedAt,
            descending: true,
            includes: [r => r.User]);

        var dtos = _mapper.Map<List<ReviewDto>>(paged.Items);
        return PaginatedResult<ReviewDto>.Success(dtos, paged.TotalCount, request.Page, request.PageSize);
    }
}

// ── Get reviews by user (my reviews) ──
public record GetUserReviewsQuery(string UserId, int Page = 1, int PageSize = 10)
    : IRequest<PaginatedResult<ReviewDto>>;

public class GetUserReviewsHandler : IRequestHandler<GetUserReviewsQuery, PaginatedResult<ReviewDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetUserReviewsHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<ReviewDto>> Handle(GetUserReviewsQuery request, CancellationToken ct)
    {
        var paged = await _uow.Reviews.GetPagedAsync(
            request.Page, request.PageSize,
            predicate: r => r.UserId == request.UserId,
            orderBy: r => r.CreatedAt,
            descending: true,
            includes: [r => r.Business]);

        var dtos = _mapper.Map<List<ReviewDto>>(paged.Items);
        return PaginatedResult<ReviewDto>.Success(dtos, paged.TotalCount, request.Page, request.PageSize);
    }
}

// ══════════════════════════════════════════════
// ── Commands ──
// ══════════════════════════════════════════════

// ── Create Review ──
public record CreateReviewCommand(CreateReviewRequest Data, string UserId) : IRequest<Result<ReviewDto>>;

public class CreateReviewHandler : IRequestHandler<CreateReviewCommand, Result<ReviewDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public CreateReviewHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<ReviewDto>> Handle(CreateReviewCommand request, CancellationToken ct)
    {
        var biz = await _uow.Businesses.GetByIdAsync(request.Data.BusinessId);
        if (biz is null)
            return Result<ReviewDto>.Failure($"Business with Id {request.Data.BusinessId} was not found.");

        // Check if user already reviewed this business
        var existing = await _uow.Reviews.AnyAsync(
            r => r.BusinessId == request.Data.BusinessId && r.UserId == request.UserId);
        if (existing)
            return Result<ReviewDto>.Failure("You have already reviewed this business.");

        var entity = _mapper.Map<Review>(request.Data);
        entity.UserId = request.UserId;
        entity.Status = ReviewStatus.Approved; // auto-approve for now

        await _uow.Reviews.AddAsync(entity);

        // Update business rating
        biz.ReviewCount += 1;
        biz.Rating = ((biz.Rating * (biz.ReviewCount - 1)) + entity.Rating) / biz.ReviewCount;

        await _uow.SaveChangesAsync(ct);
        return Result<ReviewDto>.Success(_mapper.Map<ReviewDto>(entity));
    }
}

// ── Flag Review ──
public record FlagReviewCommand(int ReviewId, string Reason, string FlaggedByUserId)
    : IRequest<Result<ReviewDto>>;

public class FlagReviewHandler : IRequestHandler<FlagReviewCommand, Result<ReviewDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public FlagReviewHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<ReviewDto>> Handle(FlagReviewCommand request, CancellationToken ct)
    {
        var entity = await _uow.Reviews.GetByIdAsync(request.ReviewId);
        if (entity is null)
            return Result<ReviewDto>.Failure($"Review with Id {request.ReviewId} was not found.");

        entity.Status = ReviewStatus.Flagged;
        entity.FlagReason = request.Reason;
        entity.FlaggedByUserId = request.FlaggedByUserId;

        await _uow.SaveChangesAsync(ct);
        return Result<ReviewDto>.Success(_mapper.Map<ReviewDto>(entity));
    }
}

// ── Approve Review (Admin) ──
public record ApproveReviewCommand(int Id) : IRequest<Result<ReviewDto>>;

public class ApproveReviewHandler : IRequestHandler<ApproveReviewCommand, Result<ReviewDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public ApproveReviewHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<ReviewDto>> Handle(ApproveReviewCommand request, CancellationToken ct)
    {
        var entity = await _uow.Reviews.GetByIdAsync(request.Id);
        if (entity is null)
            return Result<ReviewDto>.Failure($"Review with Id {request.Id} was not found.");

        entity.Status = ReviewStatus.Approved;
        entity.FlagReason = null;
        entity.FlaggedByUserId = null;

        await _uow.SaveChangesAsync(ct);
        return Result<ReviewDto>.Success(_mapper.Map<ReviewDto>(entity));
    }
}

// ── Reject Review (Admin) ──
public record RejectReviewCommand(int Id) : IRequest<Result<ReviewDto>>;

public class RejectReviewHandler : IRequestHandler<RejectReviewCommand, Result<ReviewDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public RejectReviewHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<ReviewDto>> Handle(RejectReviewCommand request, CancellationToken ct)
    {
        var entity = await _uow.Reviews.GetByIdAsync(request.Id);
        if (entity is null)
            return Result<ReviewDto>.Failure($"Review with Id {request.Id} was not found.");

        entity.Status = ReviewStatus.Rejected;
        await _uow.SaveChangesAsync(ct);
        return Result<ReviewDto>.Success(_mapper.Map<ReviewDto>(entity));
    }
}

// ── Delete Review ──
public record DeleteReviewCommand(int Id) : IRequest<Result>;

public class DeleteReviewHandler : IRequestHandler<DeleteReviewCommand, Result>
{
    private readonly IUnitOfWork _uow;

    public DeleteReviewHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(DeleteReviewCommand request, CancellationToken ct)
    {
        var entity = await _uow.Reviews.FirstOrDefaultAsync(r => r.Id == request.Id);
        if (entity is null)
            return Result.Failure($"Review with Id {request.Id} was not found.");

        // Update business rating
        var biz = await _uow.Businesses.GetByIdAsync(entity.BusinessId);
        if (biz is not null && biz.ReviewCount > 1)
        {
            biz.Rating = ((biz.Rating * biz.ReviewCount) - entity.Rating) / (biz.ReviewCount - 1);
            biz.ReviewCount -= 1;
        }
        else if (biz is not null)
        {
            biz.Rating = 0;
            biz.ReviewCount = 0;
        }

        _uow.Reviews.SoftDelete(entity);
        await _uow.SaveChangesAsync(ct);
        return Result.Success("Review deleted.");
    }
}
