using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SportMarket.Application.Common.Dtos;
using SportMarket.Application.Common.Exceptions;
using SportMarket.Application.Common.Interfaces;
using SportMarket.Domain.Entities;
using SportMarket.Domain.Enums;

namespace SportMarket.Application.Features.Orders;

public class PlaceOrderCommandValidator : AbstractValidator<PlaceOrderCommand>
{
    public PlaceOrderCommandValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("سبد خرید خالی است.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("تعداد باید بیشتر از صفر باشد.")
                .LessThanOrEqualTo(50).WithMessage("حداکثر تعداد هر پکیج ۵۰ است.");
        });
    }
}

public class PlaceOrderCommandHandler : IRequestHandler<PlaceOrderCommand, OrderDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IIdentityService _identityService;

    public PlaceOrderCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser, IIdentityService identityService)
    {
        _db = db;
        _currentUser = currentUser;
        _identityService = identityService;
    }

    public async Task<OrderDto> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedException("برای ثبت سفارش باید وارد شوید.");

        var packageIds = request.Items.Select(i => i.PackageId).Distinct().ToList();

        var packages = await _db.Packages
            .Where(p => packageIds.Contains(p.Id) && p.IsActive)
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        var order = new Order { UserId = userId };

        foreach (var item in request.Items)
        {
            if (!packages.TryGetValue(item.PackageId, out var package))
            {
                throw new NotFoundException("پکیج فعال", item.PackageId);
            }

            order.Items.Add(new OrderItem
            {
                PackageId = package.Id,
                PackageTitle = package.Title,
                UnitPrice = package.DiscountedPrice ?? package.Price,
                Quantity = item.Quantity
            });
        }

        order.TotalAmount = order.Items.Sum(i => i.UnitPrice * i.Quantity);

        _db.Orders.Add(order);
        await _db.SaveChangesAsync(cancellationToken);

        var names = await _identityService.GetUserNamesAsync(new[] { userId }, cancellationToken);
        return OrderMappings.ToDto(order, names);
    }
}

public class PayOrderCommandHandler : IRequestHandler<PayOrderCommand, OrderDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IIdentityService _identityService;

    public PayOrderCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser, IIdentityService identityService)
    {
        _db = db;
        _currentUser = currentUser;
        _identityService = identityService;
    }

    public async Task<OrderDto> Handle(PayOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await GetOrderWithAccessCheckAsync(request.Id, cancellationToken);

        if (order.Status != OrderStatus.Pending)
        {
            throw new ConflictException("این سفارش قابل پرداخت نیست.");
        }

        order.Status = OrderStatus.Paid;
        order.PaidAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        var names = await _identityService.GetUserNamesAsync(new[] { order.UserId }, cancellationToken);
        return OrderMappings.ToDto(order, names);
    }

    private async Task<Order> GetOrderWithAccessCheckAsync(Guid id, CancellationToken ct)
    {
        var order = await _db.Orders.Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, ct)
            ?? throw new NotFoundException("سفارش", id);

        var isAdmin = _currentUser.UserId is not null &&
                      await _identityService.GetSnapshotAsync(_currentUser.UserId, ct) is { } u &&
                      u.Roles.Contains("Admin");

        if (order.UserId != _currentUser.UserId && !isAdmin)
        {
            throw new UnauthorizedException("دسترسی به این سفارش مجاز نیست.");
        }

        return order;
    }
}

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, OrderDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IIdentityService _identityService;

    public CancelOrderCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser, IIdentityService identityService)
    {
        _db = db;
        _currentUser = currentUser;
        _identityService = identityService;
    }

    public async Task<OrderDto> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _db.Orders.Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("سفارش", request.Id);

        var isAdmin = _currentUser.UserId is not null &&
                      await _identityService.GetSnapshotAsync(_currentUser.UserId, cancellationToken) is { } u &&
                      u.Roles.Contains("Admin");

        if (order.UserId != _currentUser.UserId && !isAdmin)
        {
            throw new UnauthorizedException("دسترسی به این سفارش مجاز نیست.");
        }

        if (order.Status == OrderStatus.Paid)
        {
            throw new ConflictException("سفارش پرداخت‌شده قابل لغو نیست.");
        }

        order.Status = OrderStatus.Canceled;
        await _db.SaveChangesAsync(cancellationToken);

        var names = await _identityService.GetUserNamesAsync(new[] { order.UserId }, cancellationToken);
        return OrderMappings.ToDto(order, names);
    }
}

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, OrderDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IIdentityService _identityService;

    public UpdateOrderStatusCommandHandler(IApplicationDbContext db, IIdentityService identityService)
    {
        _db = db;
        _identityService = identityService;
    }

    public async Task<OrderDto> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<OrderStatus>(request.Status, ignoreCase: true, out var status))
        {
            throw new AppValidationException(new Dictionary<string, string[]>
            {
                ["Status"] = new[] { "وضعیت معتبر نیست. مقادیر مجاز: Pending، Paid، Canceled" }
            });
        }

        var order = await _db.Orders.Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("سفارش", request.Id);

        order.Status = status;
        order.PaidAtUtc = status == OrderStatus.Paid ? order.PaidAtUtc ?? DateTime.UtcNow : null;

        await _db.SaveChangesAsync(cancellationToken);

        var names = await _identityService.GetUserNamesAsync(new[] { order.UserId }, cancellationToken);
        return OrderMappings.ToDto(order, names);
    }
}

public class GetMyOrdersQueryHandler : IRequestHandler<GetMyOrdersQuery, IReadOnlyList<OrderDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IIdentityService _identityService;

    public GetMyOrdersQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser, IIdentityService identityService)
    {
        _db = db;
        _currentUser = currentUser;
        _identityService = identityService;
    }

    public async Task<IReadOnlyList<OrderDto>> Handle(GetMyOrdersQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedException("برای مشاهده سفارش‌ها باید وارد شوید.");

        var orders = await _db.Orders.AsNoTracking()
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var names = await _identityService.GetUserNamesAsync(new[] { userId }, cancellationToken);

        return orders.Select(o => OrderMappings.ToDto(o, names)).ToList();
    }
}
