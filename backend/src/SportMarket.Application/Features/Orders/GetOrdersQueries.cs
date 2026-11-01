using MediatR;
using Microsoft.EntityFrameworkCore;
using SportMarket.Application.Common.Dtos;
using SportMarket.Application.Common.Exceptions;
using SportMarket.Application.Common.Interfaces;
using SportMarket.Application.Common.Models;
using SportMarket.Application.Features.Orders;
using SportMarket.Domain.Enums;

namespace SportMarket.Application.Features.Orders;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IIdentityService _identityService;

    public GetOrderByIdQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser, IIdentityService identityService)
    {
        _db = db;
        _currentUser = currentUser;
        _identityService = identityService;
    }

    public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _db.Orders.AsNoTracking()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("سفارش", request.Id);

        var isAdmin = _currentUser.UserId is not null &&
                      await _identityService.GetSnapshotAsync(_currentUser.UserId, cancellationToken) is { } u &&
                      u.Roles.Contains("Admin");

        if (order.UserId != _currentUser.UserId && !isAdmin)
        {
            throw new UnauthorizedException("دسترسی به این سفارش مجاز نیست.");
        }

        var names = await _identityService.GetUserNamesAsync(new[] { order.UserId }, cancellationToken);
        return OrderMappings.ToDto(order, names);
    }
}

public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, PagedResult<OrderDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IIdentityService _identityService;

    public GetAllOrdersQueryHandler(IApplicationDbContext db, IIdentityService identityService)
    {
        _db = db;
        _identityService = identityService;
    }

    public async Task<PagedResult<OrderDto>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Domain.Entities.Order> query = _db.Orders.AsNoTracking().Include(o => o.Items);

        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<OrderStatus>(request.Status, ignoreCase: true, out var status))
        {
            query = query.Where(o => o.Status == status);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var orders = await query
            .OrderByDescending(o => o.CreatedAtUtc)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var names = await _identityService.GetUserNamesAsync(orders.Select(o => o.UserId).Distinct(), cancellationToken);

        return new PagedResult<OrderDto>(
            orders.Select(o => OrderMappings.ToDto(o, names)).ToList(),
            totalCount, request.Page, request.PageSize);
    }
}

public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IIdentityService _identityService;

    public GetDashboardStatsQueryHandler(IApplicationDbContext db, IIdentityService identityService)
    {
        _db = db;
        _identityService = identityService;
    }

    public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        var usersCount = await _identityService.GetUsersAsync(null, 1, 1, cancellationToken);

        var orders = _db.Orders.AsNoTracking().Include(o => o.Items);

        var ordersCount = await orders.CountAsync(cancellationToken);
        var paidOrdersCount = await orders.CountAsync(o => o.Status == OrderStatus.Paid, cancellationToken);
        var totalRevenue = await orders.Where(o => o.Status == OrderStatus.Paid)
            .SumAsync(o => (decimal?)o.TotalAmount, cancellationToken) ?? 0m;

        var packagesCount = await _db.Packages.CountAsync(cancellationToken);

        var recentOrders = await orders
            .OrderByDescending(o => o.CreatedAtUtc)
            .Take(5)
            .ToListAsync(cancellationToken);

        var names = await _identityService.GetUserNamesAsync(recentOrders.Select(o => o.UserId).Distinct(), cancellationToken);

        return new DashboardStatsDto(
            usersCount.TotalCount,
            packagesCount,
            ordersCount,
            paidOrdersCount,
            totalRevenue,
            recentOrders.Select(o => OrderMappings.ToDto(o, names)).ToList());
    }
}
