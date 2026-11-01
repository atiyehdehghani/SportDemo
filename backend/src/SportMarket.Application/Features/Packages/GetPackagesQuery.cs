using MediatR;
using Microsoft.EntityFrameworkCore;
using SportMarket.Application.Common.Dtos;
using SportMarket.Application.Common.Models;

namespace SportMarket.Application.Features.Packages;

public record GetPackagesQuery(string? Search, bool IncludeInactive = false, int Page = 1, int PageSize = 12)
    : IRequest<PagedResult<PackageDto>>;

public class GetPackagesQueryHandler : IRequestHandler<GetPackagesQuery, PagedResult<PackageDto>>
{
    private readonly IApplicationDbContext _db;

    public GetPackagesQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<PackageDto>> Handle(GetPackagesQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Packages.AsNoTracking();

        if (!request.IncludeInactive)
        {
            query = query.Where(p => p.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(p => p.Title.Contains(search) || p.Description.Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(p => p.CreatedAtUtc)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<PackageDto>(
            items.Select(PackageMappings.ToDto).ToList(),
            totalCount, request.Page, request.PageSize);
    }
}
