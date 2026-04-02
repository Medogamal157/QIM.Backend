using AutoMapper;
using MediatR;
using QIM.Application.DTOs.Content;
using QIM.Application.Interfaces;
using QIM.Domain.Common.Enums;
using QIM.Shared.Models;

namespace QIM.Application.Features.BlogPosts;

// ── Queries ──

public record GetAllBlogPostsQuery(int Page = 1, int PageSize = 10) : IRequest<PaginatedResult<BlogPostDto>>;

public class GetAllBlogPostsHandler : IRequestHandler<GetAllBlogPostsQuery, PaginatedResult<BlogPostDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetAllBlogPostsHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<BlogPostDto>> Handle(GetAllBlogPostsQuery request, CancellationToken ct)
    {
        var paged = await _uow.BlogPosts.GetPagedAsync(
            request.Page, request.PageSize,
            orderBy: b => b.CreatedAt,
            descending: true,
            includes: b => b.Author!);

        var dtos = _mapper.Map<List<BlogPostDto>>(paged.Items);
        return PaginatedResult<BlogPostDto>.Success(dtos, paged.TotalCount, request.Page, request.PageSize);
    }
}

public record GetPublishedBlogPostsQuery(int Page = 1, int PageSize = 10) : IRequest<PaginatedResult<BlogPostDto>>;

public class GetPublishedBlogPostsHandler : IRequestHandler<GetPublishedBlogPostsQuery, PaginatedResult<BlogPostDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetPublishedBlogPostsHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<BlogPostDto>> Handle(GetPublishedBlogPostsQuery request, CancellationToken ct)
    {
        var paged = await _uow.BlogPosts.GetPagedAsync(
            request.Page, request.PageSize,
            predicate: b => b.Status == BlogPostStatus.Published,
            orderBy: b => (object)b.PublishedAt!,
            descending: true,
            includes: b => b.Author!);

        var dtos = _mapper.Map<List<BlogPostDto>>(paged.Items);
        return PaginatedResult<BlogPostDto>.Success(dtos, paged.TotalCount, request.Page, request.PageSize);
    }
}

public record GetBlogPostByIdQuery(int Id) : IRequest<Result<BlogPostDto>>;

public class GetBlogPostByIdHandler : IRequestHandler<GetBlogPostByIdQuery, Result<BlogPostDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetBlogPostByIdHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<BlogPostDto>> Handle(GetBlogPostByIdQuery request, CancellationToken ct)
    {
        var post = await _uow.BlogPosts.FirstOrDefaultAsync(
            b => b.Id == request.Id,
            b => b.Author!);

        if (post is null)
            return Result<BlogPostDto>.Failure($"BlogPost with Id {request.Id} was not found.");

        return Result<BlogPostDto>.Success(_mapper.Map<BlogPostDto>(post));
    }
}

// ── Commands ──

public record CreateBlogPostCommand(CreateBlogPostRequest Data, string AuthorId) : IRequest<Result<BlogPostDto>>;

public class CreateBlogPostHandler : IRequestHandler<CreateBlogPostCommand, Result<BlogPostDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public CreateBlogPostHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<BlogPostDto>> Handle(CreateBlogPostCommand request, CancellationToken ct)
    {
        var entity = _mapper.Map<Domain.Entities.BlogPost>(request.Data);
        entity.AuthorId = request.AuthorId;
        entity.Status = BlogPostStatus.Draft;

        await _uow.BlogPosts.AddAsync(entity);
        await _uow.SaveChangesAsync(ct);

        return Result<BlogPostDto>.Success(_mapper.Map<BlogPostDto>(entity));
    }
}

public record UpdateBlogPostCommand(int Id, UpdateBlogPostRequest Data) : IRequest<Result<BlogPostDto>>;

public class UpdateBlogPostHandler : IRequestHandler<UpdateBlogPostCommand, Result<BlogPostDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public UpdateBlogPostHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<BlogPostDto>> Handle(UpdateBlogPostCommand request, CancellationToken ct)
    {
        var entity = await _uow.BlogPosts.GetByIdAsync(request.Id);
        if (entity is null)
            return Result<BlogPostDto>.Failure($"BlogPost with Id {request.Id} was not found.");

        _mapper.Map(request.Data, entity);
        await _uow.SaveChangesAsync(ct);
        return Result<BlogPostDto>.Success(_mapper.Map<BlogPostDto>(entity));
    }
}

public record DeleteBlogPostCommand(int Id) : IRequest<Result>;

public class DeleteBlogPostHandler : IRequestHandler<DeleteBlogPostCommand, Result>
{
    private readonly IUnitOfWork _uow;

    public DeleteBlogPostHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(DeleteBlogPostCommand request, CancellationToken ct)
    {
        var entity = await _uow.BlogPosts.GetByIdAsync(request.Id);
        if (entity is null)
            return Result.Failure($"BlogPost with Id {request.Id} was not found.");

        _uow.BlogPosts.SoftDelete(entity);
        await _uow.SaveChangesAsync(ct);
        return Result.Success("Blog post deleted.");
    }
}

public record ToggleBlogPostPublishCommand(int Id) : IRequest<Result<BlogPostDto>>;

public class ToggleBlogPostPublishHandler : IRequestHandler<ToggleBlogPostPublishCommand, Result<BlogPostDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public ToggleBlogPostPublishHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<BlogPostDto>> Handle(ToggleBlogPostPublishCommand request, CancellationToken ct)
    {
        var entity = await _uow.BlogPosts.GetByIdAsync(request.Id);
        if (entity is null)
            return Result<BlogPostDto>.Failure($"BlogPost with Id {request.Id} was not found.");

        if (entity.Status == BlogPostStatus.Published)
        {
            entity.Status = BlogPostStatus.Draft;
            entity.PublishedAt = null;
        }
        else
        {
            entity.Status = BlogPostStatus.Published;
            entity.PublishedAt = DateTime.UtcNow;
        }

        await _uow.SaveChangesAsync(ct);
        return Result<BlogPostDto>.Success(_mapper.Map<BlogPostDto>(entity));
    }
}
