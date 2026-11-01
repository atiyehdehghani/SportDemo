using MediatR;
using SportMarket.Application.Common.Dtos;
using SportMarket.Application.Common.Models;

namespace SportMarket.Application.Features.Orders;

/// <summary>مپر مشترک سفارش — نام کاربر از دیکشنری شناسه→نام پر می‌شود</summary>
public static class OrderMappings
{
    public static OrderItemDto ToDto(Domain.Entities.OrderItem i) =>
        new(i.PackageId, i.PackageTitle, i.UnitPrice, i.Quantity);

    public static OrderDto ToDto(Domain.Entities.Order o, IReadOnlyDictionary<string, string> userNames) =>
        new(
            o.Id,
            o.UserId,
            userNames.TryGetValue(o.UserId, out var name) ? name : "کاربر حذف‌شده",
            o.TotalAmount,
            o.Status.ToString(),
            o.CreatedAtUtc,
            o.PaidAtUtc,
            o.Items.Select(ToDto).ToList());
}

public record PlaceOrderItemInput(Guid PackageId, int Quantity);

public record PlaceOrderCommand(IReadOnlyList<PlaceOrderItemInput> Items) : IRequest<OrderDto>;

public record PayOrderCommand(Guid Id) : IRequest<OrderDto>;

public record CancelOrderCommand(Guid Id) : IRequest<OrderDto>;

public record UpdateOrderStatusCommand(Guid Id, string Status) : IRequest<OrderDto>;

public record GetMyOrdersQuery : IRequest<IReadOnlyList<OrderDto>>;

public record GetOrderByIdQuery(Guid Id) : IRequest<OrderDto>;

public record GetAllOrdersQuery(string? Status, int Page = 1, int PageSize = 20) : IRequest<PagedResult<OrderDto>>;

public record GetDashboardStatsQuery : IRequest<DashboardStatsDto>;
