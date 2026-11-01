using MediatR;
using Microsoft.EntityFrameworkCore;
using SportMarket.Application.Common.Dtos;
using SportMarket.Application.Common.Exceptions;
using SportMarket.Application.Features.Articles;

namespace SportMarket.Application.Features.Articles;

public record GetArticlesQuery(string? Search, bool IncludeUnpublished = false, int Page = 1, int PageSize = 12)
    : IRequest<PagedResult<ArticleDto>>;

public class GetArticlesQueryHandler : IRequestHandler<GetArticlesQuery, PagedResult<ArticleDto>>
{
    private readonly IApplicationDbContext _db;

    public GetArticlesQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<ArticleDto>> Handle(GetArticlesQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Articles.AsNoTracking();

        if (!request.IncludeUnpublished)
        {
            query = query.Where(a => a.IsPublished);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(a => a.Title.Contains(search) || a.Summary.Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(a => a.PublishedAtUtc ?? a.CreatedAtUtc)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<ArticleDto>(
            items.Select(ArticleMappings.ToDto).ToList(),
            totalCount, request.Page, request.PageSize);
    }
}

public record GetArticleBySlugQuery(string Slug, bool IncludeUnpublished = false) : IRequest<ArticleDto>;

public class GetArticleBySlugQueryHandler : IRequestHandler<GetArticleBySlugQuery, ArticleDto>
{
    private readonly IApplicationDbContext _db;

    public GetArticleBySlugQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ArticleDto> Handle(GetArticleBySlugQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Articles.AsNoTracking();

        if (!request.IncludeUnpublished)
        {
            query = query.Where(a => a.IsPublished);
        }

        var article = await query.FirstOrDefaultAsync(a => a.Slug == request.Slug, cancellationToken)
            ?? throw new NotFoundException("مقاله", request.Slug);

        return ArticleMappings.ToDto(article);
    }
}

public record GetArticleByIdQuery(Guid Id) : IRequest<ArticleDto>;

public class GetArticleByIdQueryHandler : IRequestHandler<GetArticleByIdQuery, ArticleDto>
{
    private readonly IApplicationDbContext _db;

    public GetArticleByIdQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ArticleDto> Handle(GetArticleByIdQuery request, CancellationToken cancellationToken)
    {
        var article = await _db.Articles.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("مقاله", request.Id);

        return ArticleMappings.ToDto(article);
    }
}
