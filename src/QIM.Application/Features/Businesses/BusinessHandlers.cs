using System.Linq.Expressions;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QIM.Application.DTOs.Business;
using QIM.Application.Interfaces;
using QIM.Domain.Common.Enums;
using QIM.Domain.Entities;
using QIM.Shared.Models;

namespace QIM.Application.Features.Businesses;

// ══════════════════════════════════════════════
// ── Queries ──
// ══════════════════════════════════════════════

// ── Get All (Admin) ──
public record GetAllBusinessesQuery(int Page = 1, int PageSize = 10, BusinessStatus? Status = null)
    : IRequest<PaginatedResult<BusinessListDto>>;

public class GetAllBusinessesHandler : IRequestHandler<GetAllBusinessesQuery, PaginatedResult<BusinessListDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetAllBusinessesHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<BusinessListDto>> Handle(GetAllBusinessesQuery request, CancellationToken ct)
    {
        var paged = await _uow.Businesses.GetPagedAsync(
            request.Page, request.PageSize,
            predicate: request.Status.HasValue ? b => b.Status == request.Status.Value : null,
            orderBy: b => b.CreatedAt,
            descending: true,
            includes: [b => b.Activity, b => (object)b.Addresses]);

        var dtos = _mapper.Map<List<BusinessListDto>>(paged.Items);
        return PaginatedResult<BusinessListDto>.Success(dtos, paged.TotalCount, request.Page, request.PageSize);
    }
}

// ── Get By Id (detail) ──
public record GetBusinessByIdQuery(int Id) : IRequest<Result<BusinessDto>>;

public class GetBusinessByIdHandler : IRequestHandler<GetBusinessByIdQuery, Result<BusinessDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetBusinessByIdHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<BusinessDto>> Handle(GetBusinessByIdQuery request, CancellationToken ct)
    {
        var biz = await _uow.Businesses.FirstOrDefaultAsync(
            b => b.Id == request.Id,
            q => q.Include(b => b.Owner)
                  .Include(b => b.Activity)
                  .Include(b => b.Speciality)
                  .Include(b => b.Addresses).ThenInclude(a => a.Country)
                  .Include(b => b.Addresses).ThenInclude(a => a.City)
                  .Include(b => b.Addresses).ThenInclude(a => a.District)
                  .Include(b => b.WorkHours)
                  .Include(b => b.Images)
                  .Include(b => b.Keywords));

        if (biz is null)
            return Result<BusinessDto>.Failure($"Business with Id {request.Id} was not found.");

        return Result<BusinessDto>.Success(_mapper.Map<BusinessDto>(biz));
    }
}

// ── Get By Account Code (public) ──
public record GetBusinessByAccountCodeQuery(string AccountCode) : IRequest<Result<BusinessDto>>;

public class GetBusinessByAccountCodeHandler : IRequestHandler<GetBusinessByAccountCodeQuery, Result<BusinessDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetBusinessByAccountCodeHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<BusinessDto>> Handle(GetBusinessByAccountCodeQuery request, CancellationToken ct)
    {
        var biz = await _uow.Businesses.FirstOrDefaultAsync(
            b => b.AccountCode == request.AccountCode,
            q => q.Include(b => b.Owner)
                  .Include(b => b.Activity)
                  .Include(b => b.Speciality)
                  .Include(b => b.Addresses).ThenInclude(a => a.Country)
                  .Include(b => b.Addresses).ThenInclude(a => a.City)
                  .Include(b => b.Addresses).ThenInclude(a => a.District)
                  .Include(b => b.WorkHours)
                  .Include(b => b.Images)
                  .Include(b => b.Keywords));

        if (biz is null)
            return Result<BusinessDto>.Failure($"Business with AccountCode '{request.AccountCode}' was not found.");

        return Result<BusinessDto>.Success(_mapper.Map<BusinessDto>(biz));
    }
}

// ── Get Businesses by Owner ──
public record GetBusinessesByOwnerQuery(string OwnerId, int Page = 1, int PageSize = 10)
    : IRequest<PaginatedResult<BusinessListDto>>;

public class GetBusinessesByOwnerHandler : IRequestHandler<GetBusinessesByOwnerQuery, PaginatedResult<BusinessListDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetBusinessesByOwnerHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<BusinessListDto>> Handle(GetBusinessesByOwnerQuery request, CancellationToken ct)
    {
        var paged = await _uow.Businesses.GetPagedAsync(
            request.Page, request.PageSize,
            predicate: b => b.OwnerId == request.OwnerId,
            orderBy: b => b.CreatedAt,
            descending: true,
            includes: [b => b.Activity]);

        var dtos = _mapper.Map<List<BusinessListDto>>(paged.Items);
        return PaginatedResult<BusinessListDto>.Success(dtos, paged.TotalCount, request.Page, request.PageSize);
    }
}

// ── Search Businesses (Public) ──
public record SearchBusinessesQuery(
    string? Keyword = null,
    int? ActivityId = null,
    int? SpecialityId = null,
    int? CountryId = null,
    int? CityId = null,
    int? DistrictId = null,
    double? MinRating = null,
    int? MinReviewCount = null,
    SearchIn SearchIn = SearchIn.All,
    SortBy SortBy = SortBy.HighestRated,
    int Page = 1,
    int PageSize = 10) : IRequest<PaginatedResult<BusinessListDto>>;

public class SearchBusinessesHandler : IRequestHandler<SearchBusinessesQuery, PaginatedResult<BusinessListDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public SearchBusinessesHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<BusinessListDto>> Handle(SearchBusinessesQuery request, CancellationToken ct)
    {
        Expression<Func<Business, bool>> predicate = b =>
            b.Status == BusinessStatus.Approved
            && (request.Keyword == null
                || (request.SearchIn == SearchIn.CompanyName && (b.NameAr.Contains(request.Keyword) || b.NameEn.Contains(request.Keyword)))
                || (request.SearchIn == SearchIn.ActivityCode && b.AccountCode != null && b.AccountCode.Contains(request.Keyword))
                || (request.SearchIn == SearchIn.Keywords && b.Keywords.Any(k => k.Keyword.Contains(request.Keyword)))
                || (request.SearchIn == SearchIn.ActivityName && (b.Activity.NameAr.Contains(request.Keyword) || b.Activity.NameEn.Contains(request.Keyword)))
                || (request.SearchIn == SearchIn.All && (b.NameAr.Contains(request.Keyword) || b.NameEn.Contains(request.Keyword) || (b.DescriptionAr != null && b.DescriptionAr.Contains(request.Keyword)) || (b.DescriptionEn != null && b.DescriptionEn.Contains(request.Keyword)) || (b.AccountCode != null && b.AccountCode.Contains(request.Keyword)) || b.Keywords.Any(k => k.Keyword.Contains(request.Keyword)) || b.Activity.NameAr.Contains(request.Keyword) || b.Activity.NameEn.Contains(request.Keyword))))
            && (!request.ActivityId.HasValue || b.ActivityId == request.ActivityId)
            && (!request.SpecialityId.HasValue || b.SpecialityId == request.SpecialityId)
            && (!request.MinRating.HasValue || b.Rating >= request.MinRating)
            && (!request.MinReviewCount.HasValue || b.ReviewCount >= request.MinReviewCount)
            && (!request.CountryId.HasValue || b.Addresses.Any(a => a.CountryId == request.CountryId))
            && (!request.CityId.HasValue || b.Addresses.Any(a => a.CityId == request.CityId))
            && (!request.DistrictId.HasValue || b.Addresses.Any(a => a.DistrictId == request.DistrictId));

        Expression<Func<Business, object>> orderBy = request.SortBy switch
        {
            SortBy.MostReviews => b => b.ReviewCount,
            SortBy.Newest => b => b.CreatedAt,
            _ => b => b.Rating
        };

        var paged = await _uow.Businesses.GetPagedAsync(
            request.Page, request.PageSize,
            predicate: predicate,
            orderBy: orderBy,
            descending: true,
            includes: [b => b.Activity, b => (object)b.Addresses]);

        var dtos = _mapper.Map<List<BusinessListDto>>(paged.Items);
        return PaginatedResult<BusinessListDto>.Success(dtos, paged.TotalCount, request.Page, request.PageSize);
    }
}

// ── Autocomplete Businesses (Public) ──
public record AutocompleteBusinessesQuery(string Query, int Limit = 10, SearchIn SearchIn = SearchIn.All) : IRequest<Result<List<BusinessAutoCompleteDto>>>;

public class AutocompleteBusinessesHandler : IRequestHandler<AutocompleteBusinessesQuery, Result<List<BusinessAutoCompleteDto>>>
{
    private readonly IUnitOfWork _uow;

    public AutocompleteBusinessesHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<List<BusinessAutoCompleteDto>>> Handle(AutocompleteBusinessesQuery request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Query) || request.Query.Length < 2)
            return Result<List<BusinessAutoCompleteDto>>.Success(new List<BusinessAutoCompleteDto>());

        var q = request.Query;
        var limit = request.Limit;
        List<BusinessAutoCompleteDto> dtos;

        switch (request.SearchIn)
        {
            case SearchIn.CompanyName:
            {
                var paged = await _uow.Businesses.GetPagedAsync(
                    1, limit,
                    predicate: b => b.Status == BusinessStatus.Approved
                        && (b.NameAr.Contains(q) || b.NameEn.Contains(q)),
                    orderBy: b => b.Rating,
                    descending: true);
                dtos = paged.Items.Select(b => new BusinessAutoCompleteDto
                {
                    Id = b.Id, NameAr = b.NameAr, NameEn = b.NameEn
                }).ToList();
                break;
            }
            case SearchIn.ActivityName:
            {
                var paged = await _uow.Activities.GetPagedAsync(
                    1, limit,
                    predicate: c => c.IsEnabled
                        && (c.NameAr.Contains(q) || c.NameEn.Contains(q)),
                    orderBy: c => c.SortOrder);
                dtos = paged.Items.Select(c => new BusinessAutoCompleteDto
                {
                    Id = c.Id, NameAr = c.NameAr, NameEn = c.NameEn
                }).ToList();
                break;
            }
            case SearchIn.ActivityCode:
            {
                var paged = await _uow.Businesses.GetPagedAsync(
                    1, limit * 3,
                    predicate: b => b.Status == BusinessStatus.Approved
                        && b.AccountCode != null && b.AccountCode.Contains(q),
                    orderBy: b => b.Rating,
                    descending: true);
                dtos = paged.Items
                    .Where(b => b.AccountCode != null)
                    .GroupBy(b => b.AccountCode!)
                    .Take(limit)
                    .Select(g => new BusinessAutoCompleteDto
                    {
                        Id = g.First().Id, NameAr = g.Key, NameEn = g.Key
                    }).ToList();
                break;
            }
            case SearchIn.Keywords:
            {
                var paged = await _uow.BusinessKeywords.GetPagedAsync(
                    1, limit * 3,
                    predicate: k => k.Keyword.Contains(q)
                        && k.Business.Status == BusinessStatus.Approved);
                dtos = paged.Items
                    .GroupBy(k => k.Keyword)
                    .Take(limit)
                    .Select(g => new BusinessAutoCompleteDto
                    {
                        Id = g.First().Id, NameAr = g.Key, NameEn = g.Key
                    }).ToList();
                break;
            }
            default: // SearchIn.All — search businesses across all fields
            {
                var paged = await _uow.Businesses.GetPagedAsync(
                    1, limit,
                    predicate: b => b.Status == BusinessStatus.Approved
                        && (b.NameAr.Contains(q) || b.NameEn.Contains(q)
                            || (b.AccountCode != null && b.AccountCode.Contains(q))
                            || b.Keywords.Any(k => k.Keyword.Contains(q))
                            || b.Activity.NameAr.Contains(q) || b.Activity.NameEn.Contains(q)),
                    orderBy: b => b.Rating,
                    descending: true,
                    includes: [b => b.Activity]);
                dtos = paged.Items.Select(b => new BusinessAutoCompleteDto
                {
                    Id = b.Id, NameAr = b.NameAr, NameEn = b.NameEn
                }).ToList();
                break;
            }
        }

        return Result<List<BusinessAutoCompleteDto>>.Success(dtos);
    }
}

// ── Top Businesses (Public) ──
public record GetTopBusinessesQuery(int Count = 10) : IRequest<Result<List<BusinessListDto>>>;

public class GetTopBusinessesHandler : IRequestHandler<GetTopBusinessesQuery, Result<List<BusinessListDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetTopBusinessesHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<List<BusinessListDto>>> Handle(GetTopBusinessesQuery request, CancellationToken ct)
    {
        var paged = await _uow.Businesses.GetPagedAsync(
            1, request.Count,
            predicate: b => b.Status == BusinessStatus.Approved,
            orderBy: b => b.Rating,
            descending: true,
            includes: [b => b.Activity]);

        var dtos = _mapper.Map<List<BusinessListDto>>(paged.Items);
        return Result<List<BusinessListDto>>.Success(dtos);
    }
}

// ══════════════════════════════════════════════
// ── Commands ──
// ══════════════════════════════════════════════

// ── Create Business ──
public record CreateBusinessCommand(CreateBusinessRequest Data, string OwnerId) : IRequest<Result<BusinessDto>>;

public class CreateBusinessHandler : IRequestHandler<CreateBusinessCommand, Result<BusinessDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public CreateBusinessHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<BusinessDto>> Handle(CreateBusinessCommand request, CancellationToken ct)
    {
        var entity = _mapper.Map<Business>(request.Data);
        entity.OwnerId = request.OwnerId;
        entity.Status = BusinessStatus.Pending;
        entity.Rating = 0;
        entity.ReviewCount = 0;

        // Set AccountCode to the first phone number
        if (!string.IsNullOrWhiteSpace(entity.Phones))
        {
            var raw = entity.Phones.Trim();
            if (raw.StartsWith("["))
            {
                var arr = System.Text.Json.JsonSerializer.Deserialize<string[]>(raw);
                entity.AccountCode = arr?.FirstOrDefault()?.Trim();
            }
            else
            {
                entity.AccountCode = raw.Split(',')[0].Trim();
            }
        }

        await _uow.Businesses.AddAsync(entity);
        await _uow.SaveChangesAsync(ct);

        // Save keywords
        if (request.Data.Keywords is { Count: > 0 })
        {
            foreach (var kw in request.Data.Keywords.Where(k => !string.IsNullOrWhiteSpace(k)).Distinct())
            {
                await _uow.BusinessKeywords.AddAsync(new BusinessKeyword
                {
                    BusinessId = entity.Id,
                    Keyword = kw.Trim()
                });
            }
            await _uow.SaveChangesAsync(ct);
        }

        return Result<BusinessDto>.Success(_mapper.Map<BusinessDto>(entity));
    }
}

// ── Update Business ──
public record UpdateBusinessCommand(int Id, UpdateBusinessRequest Data, string RequesterId)
    : IRequest<Result<BusinessDto>>;

public class UpdateBusinessHandler : IRequestHandler<UpdateBusinessCommand, Result<BusinessDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public UpdateBusinessHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<BusinessDto>> Handle(UpdateBusinessCommand request, CancellationToken ct)
    {
        var entity = await _uow.Businesses.GetByIdAsync(request.Id);
        if (entity is null)
            return Result<BusinessDto>.Failure($"Business with Id {request.Id} was not found.");

        // Ownership guard – only the owner can update their business
        if (entity.OwnerId != request.RequesterId)
            return Result<BusinessDto>.Failure("You are not authorized to update this business.");

        var d = request.Data;
        if (d.NameAr is not null) entity.NameAr = d.NameAr;
        if (d.NameEn is not null) entity.NameEn = d.NameEn;
        if (d.DescriptionAr is not null) entity.DescriptionAr = d.DescriptionAr;
        if (d.DescriptionEn is not null) entity.DescriptionEn = d.DescriptionEn;
        if (d.ActivityId.HasValue) entity.ActivityId = d.ActivityId.Value;
        if (d.SpecialityId.HasValue) entity.SpecialityId = d.SpecialityId.Value;
        if (d.Email is not null) entity.Email = d.Email;
        if (d.Website is not null) entity.Website = d.Website;
        if (d.Facebook is not null) entity.Facebook = d.Facebook;
        if (d.Instagram is not null) entity.Instagram = d.Instagram;
        if (d.WhatsApp is not null) entity.WhatsApp = d.WhatsApp;
        if (d.Phones is not null) entity.Phones = d.Phones;

        // Update keywords if provided
        if (d.Keywords is not null)
        {
            var existingKeywords = await _uow.BusinessKeywords.GetAllAsync(k => k.BusinessId == request.Id);
            foreach (var existing in existingKeywords)
                _uow.BusinessKeywords.SoftDelete(existing);

            foreach (var kw in d.Keywords.Where(k => !string.IsNullOrWhiteSpace(k)).Distinct())
            {
                await _uow.BusinessKeywords.AddAsync(new BusinessKeyword
                {
                    BusinessId = request.Id,
                    Keyword = kw.Trim()
                });
            }
        }

        _uow.Businesses.Update(entity);
        await _uow.SaveChangesAsync(ct);
        return Result<BusinessDto>.Success(_mapper.Map<BusinessDto>(entity));
    }
}

// ── Delete Business ──
public record DeleteBusinessCommand(int Id) : IRequest<Result>;

public class DeleteBusinessHandler : IRequestHandler<DeleteBusinessCommand, Result>
{
    private readonly IUnitOfWork _uow;

    public DeleteBusinessHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(DeleteBusinessCommand request, CancellationToken ct)
    {
        var entity = await _uow.Businesses.GetByIdAsync(request.Id);
        if (entity is null)
            return Result.Failure($"Business with Id {request.Id} was not found.");

        _uow.Businesses.SoftDelete(entity);
        await _uow.SaveChangesAsync(ct);
        return Result.Success("Business deleted.");
    }
}

// ── Approve / Reject Business (Admin) ──
public record ApproveBusinessCommand(int Id) : IRequest<Result<BusinessDto>>;

public class ApproveBusinessHandler : IRequestHandler<ApproveBusinessCommand, Result<BusinessDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public ApproveBusinessHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<BusinessDto>> Handle(ApproveBusinessCommand request, CancellationToken ct)
    {
        var entity = await _uow.Businesses.GetByIdAsync(request.Id);
        if (entity is null)
            return Result<BusinessDto>.Failure($"Business with Id {request.Id} was not found.");

        entity.Status = BusinessStatus.Approved;
        await _uow.SaveChangesAsync(ct);
        return Result<BusinessDto>.Success(_mapper.Map<BusinessDto>(entity));
    }
}

public record RejectBusinessCommand(int Id, string? Reason = null) : IRequest<Result<BusinessDto>>;

public class RejectBusinessHandler : IRequestHandler<RejectBusinessCommand, Result<BusinessDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public RejectBusinessHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<BusinessDto>> Handle(RejectBusinessCommand request, CancellationToken ct)
    {
        var entity = await _uow.Businesses.GetByIdAsync(request.Id);
        if (entity is null)
            return Result<BusinessDto>.Failure($"Business with Id {request.Id} was not found.");

        entity.Status = BusinessStatus.Rejected;
        entity.RejectionReason = request.Reason;
        await _uow.SaveChangesAsync(ct);
        return Result<BusinessDto>.Success(_mapper.Map<BusinessDto>(entity));
    }
}

// ══════════════════════════════════════════════
// ── Provider Renew / Close Business ──
// ══════════════════════════════════════════════

public record RenewBusinessCommand(int Id, string RequesterId) : IRequest<Result<BusinessDto>>;

public class RenewBusinessHandler : IRequestHandler<RenewBusinessCommand, Result<BusinessDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public RenewBusinessHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<BusinessDto>> Handle(RenewBusinessCommand request, CancellationToken ct)
    {
        var entity = await _uow.Businesses.GetByIdAsync(request.Id);
        if (entity is null)
            return Result<BusinessDto>.Failure($"Business with Id {request.Id} was not found.");
        if (entity.OwnerId != request.RequesterId)
            return Result<BusinessDto>.Failure("You are not the owner of this business.");
        if (entity.Status != BusinessStatus.Suspended && entity.Status != BusinessStatus.Rejected)
            return Result<BusinessDto>.Failure("Only suspended or rejected businesses can be renewed.");

        entity.Status = BusinessStatus.Pending;
        entity.RejectionReason = null;
        await _uow.SaveChangesAsync(ct);
        return Result<BusinessDto>.Success(_mapper.Map<BusinessDto>(entity));
    }
}

public record CloseBusinessCommand(int Id, string RequesterId) : IRequest<Result<BusinessDto>>;

public class CloseBusinessHandler : IRequestHandler<CloseBusinessCommand, Result<BusinessDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public CloseBusinessHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<BusinessDto>> Handle(CloseBusinessCommand request, CancellationToken ct)
    {
        var entity = await _uow.Businesses.GetByIdAsync(request.Id);
        if (entity is null)
            return Result<BusinessDto>.Failure($"Business with Id {request.Id} was not found.");
        if (entity.OwnerId != request.RequesterId)
            return Result<BusinessDto>.Failure("You are not the owner of this business.");
        if (entity.Status == BusinessStatus.Suspended)
            return Result<BusinessDto>.Failure("Business is already suspended.");

        entity.Status = BusinessStatus.Suspended;
        await _uow.SaveChangesAsync(ct);
        return Result<BusinessDto>.Success(_mapper.Map<BusinessDto>(entity));
    }
}

// ══════════════════════════════════════════════
// ── Business Address Handlers ──
// ══════════════════════════════════════════════

public record AddBusinessAddressCommand(int BusinessId, CreateBusinessAddressRequest Data)
    : IRequest<Result<BusinessAddressDto>>;

public class AddBusinessAddressHandler : IRequestHandler<AddBusinessAddressCommand, Result<BusinessAddressDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public AddBusinessAddressHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<BusinessAddressDto>> Handle(AddBusinessAddressCommand request, CancellationToken ct)
    {
        var biz = await _uow.Businesses.GetByIdAsync(request.BusinessId);
        if (biz is null)
            return Result<BusinessAddressDto>.Failure($"Business with Id {request.BusinessId} was not found.");

        var entity = _mapper.Map<BusinessAddress>(request.Data);
        entity.BusinessId = request.BusinessId;

        await _uow.BusinessAddresses.AddAsync(entity);
        await _uow.SaveChangesAsync(ct);

        return Result<BusinessAddressDto>.Success(_mapper.Map<BusinessAddressDto>(entity));
    }
}

public record UpdateBusinessAddressCommand(int Id, UpdateBusinessAddressRequest Data)
    : IRequest<Result<BusinessAddressDto>>;

public class UpdateBusinessAddressHandler : IRequestHandler<UpdateBusinessAddressCommand, Result<BusinessAddressDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public UpdateBusinessAddressHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<BusinessAddressDto>> Handle(UpdateBusinessAddressCommand request, CancellationToken ct)
    {
        var entity = await _uow.BusinessAddresses.GetByIdAsync(request.Id);
        if (entity is null)
            return Result<BusinessAddressDto>.Failure($"BusinessAddress with Id {request.Id} was not found.");

        _mapper.Map(request.Data, entity);
        await _uow.SaveChangesAsync(ct);
        return Result<BusinessAddressDto>.Success(_mapper.Map<BusinessAddressDto>(entity));
    }
}

public record DeleteBusinessAddressCommand(int Id) : IRequest<Result>;

public class DeleteBusinessAddressHandler : IRequestHandler<DeleteBusinessAddressCommand, Result>
{
    private readonly IUnitOfWork _uow;

    public DeleteBusinessAddressHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(DeleteBusinessAddressCommand request, CancellationToken ct)
    {
        var entity = await _uow.BusinessAddresses.GetByIdAsync(request.Id);
        if (entity is null)
            return Result.Failure($"BusinessAddress with Id {request.Id} was not found.");

        _uow.BusinessAddresses.Delete(entity);
        await _uow.SaveChangesAsync(ct);
        return Result.Success("Address deleted.");
    }
}

// ══════════════════════════════════════════════
// ── Business Work Hours Handlers ──
// ══════════════════════════════════════════════

public record SetBusinessWorkHoursCommand(int BusinessId, List<SetWorkHoursRequest> Items)
    : IRequest<Result<List<BusinessWorkHoursDto>>>;

public class SetBusinessWorkHoursHandler : IRequestHandler<SetBusinessWorkHoursCommand, Result<List<BusinessWorkHoursDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public SetBusinessWorkHoursHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<List<BusinessWorkHoursDto>>> Handle(SetBusinessWorkHoursCommand request, CancellationToken ct)
    {
        var biz = await _uow.Businesses.GetByIdAsync(request.BusinessId);
        if (biz is null)
            return Result<List<BusinessWorkHoursDto>>.Failure($"Business with Id {request.BusinessId} was not found.");

        // Remove existing work hours for this business
        var existing = await _uow.BusinessWorkHoursRepo.GetAllAsync(wh => wh.BusinessId == request.BusinessId);
        foreach (var ex in existing)
            _uow.BusinessWorkHoursRepo.Delete(ex);

        // Add new ones
        var entities = new List<BusinessWorkHours>();
        foreach (var item in request.Items)
        {
            var wh = _mapper.Map<BusinessWorkHours>(item);
            wh.BusinessId = request.BusinessId;
            await _uow.BusinessWorkHoursRepo.AddAsync(wh);
            entities.Add(wh);
        }

        await _uow.SaveChangesAsync(ct);
        return Result<List<BusinessWorkHoursDto>>.Success(_mapper.Map<List<BusinessWorkHoursDto>>(entities));
    }
}

public record GetBusinessWorkHoursQuery(int BusinessId) : IRequest<Result<List<BusinessWorkHoursDto>>>;

public class GetBusinessWorkHoursHandler : IRequestHandler<GetBusinessWorkHoursQuery, Result<List<BusinessWorkHoursDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetBusinessWorkHoursHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<List<BusinessWorkHoursDto>>> Handle(GetBusinessWorkHoursQuery request, CancellationToken ct)
    {
        var items = await _uow.BusinessWorkHoursRepo.GetAllAsync(wh => wh.BusinessId == request.BusinessId);
        return Result<List<BusinessWorkHoursDto>>.Success(_mapper.Map<List<BusinessWorkHoursDto>>(items));
    }
}

// ══════════════════════════════════════════════
// ── Business Image Handlers ──
// ══════════════════════════════════════════════

public record AddBusinessImageCommand(int BusinessId, string ImageUrl, bool IsCover, int SortOrder)
    : IRequest<Result<BusinessImageDto>>;

public class AddBusinessImageHandler : IRequestHandler<AddBusinessImageCommand, Result<BusinessImageDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public AddBusinessImageHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<BusinessImageDto>> Handle(AddBusinessImageCommand request, CancellationToken ct)
    {
        var biz = await _uow.Businesses.GetByIdAsync(request.BusinessId);
        if (biz is null)
            return Result<BusinessImageDto>.Failure($"Business with Id {request.BusinessId} was not found.");

        var entity = new BusinessImage
        {
            BusinessId = request.BusinessId,
            ImageUrl = request.ImageUrl,
            IsCover = request.IsCover,
            SortOrder = request.SortOrder
        };

        await _uow.BusinessImages.AddAsync(entity);
        await _uow.SaveChangesAsync(ct);

        return Result<BusinessImageDto>.Success(_mapper.Map<BusinessImageDto>(entity));
    }
}

public record DeleteBusinessImageCommand(int Id) : IRequest<Result>;

public class DeleteBusinessImageHandler : IRequestHandler<DeleteBusinessImageCommand, Result>
{
    private readonly IUnitOfWork _uow;

    public DeleteBusinessImageHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(DeleteBusinessImageCommand request, CancellationToken ct)
    {
        var entity = await _uow.BusinessImages.GetByIdAsync(request.Id);
        if (entity is null)
            return Result.Failure($"BusinessImage with Id {request.Id} was not found.");

        _uow.BusinessImages.Delete(entity);
        await _uow.SaveChangesAsync(ct);
        return Result.Success("Image deleted.");
    }
}
