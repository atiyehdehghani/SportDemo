using MediatR;
using Microsoft.EntityFrameworkCore;
using SportMarket.Application.Common.Dtos;
using SportMarket.Application.Common.Exceptions;
using SportMarket.Application.Common.Interfaces;

namespace SportMarket.Application.Features.Packages;

public record GetPackageBySlugQuery(string Slug, bool IncludeInactive = false) : IRequest<PackageDto>;

public class GetPackageBySlugQueryHandler : IRequestHandler<GetPackageBySlugQuery, PackageDto>
{
    private readonly IApplicationDbContext _db;

    public GetPackageBySlugQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PackageDto> Handle(GetPackageBySlugQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Packages.AsNoTracking();

        if (!request.IncludeInactive)
        {
            query = query.Where(p => p.IsActive);
        }

        var package = await query.FirstOrDefaultAsync(p => p.Slug == request.Slug, cancellationToken)
            ?? throw new NotFoundException("پکیج", request.Slug);

        return PackageMappings.ToDto(package);
    }
}

public record GetPackageByIdQuery(Guid Id) : IRequest<PackageDto>;

public class GetPackageByIdQueryHandler : IRequestHandler<GetPackageByIdQuery, PackageDto>
{
    private readonly IApplicationDbContext _db;

    public GetPackageByIdQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PackageDto> Handle(GetPackageByIdQuery request, CancellationToken cancellationToken)
    {
        var package = await _db.Packages.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("پکیج", request.Id);

        return PackageMappings.ToDto(package);
    }
}
