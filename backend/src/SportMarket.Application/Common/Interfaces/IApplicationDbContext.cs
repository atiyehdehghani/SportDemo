using Microsoft.EntityFrameworkCore;
using SportMarket.Domain.Entities;

namespace SportMarket.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Package> Packages { get; }
    DbSet<Article> Articles { get; }
    DbSet<Slider> Sliders { get; }
    DbSet<Order> Orders { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
