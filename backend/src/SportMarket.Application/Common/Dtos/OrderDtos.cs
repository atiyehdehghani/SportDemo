namespace SportMarket.Application.Common.Dtos;

public record OrderItemDto(
    Guid PackageId,
    string PackageTitle,
    decimal UnitPrice,
    int Quantity);

public record OrderDto(
    Guid Id,
    string UserId,
    string UserName,
    decimal TotalAmount,
    string Status,
    DateTime CreatedAtUtc,
    DateTime? PaidAtUtc,
    IReadOnlyList<OrderItemDto> Items);

public record AuthResponseDto(string Token, DateTime ExpiresAtUtc, UserDto User);

public record DashboardStatsDto(
    int UsersCount,
    int PackagesCount,
    int OrdersCount,
    int PaidOrdersCount,
    decimal TotalRevenue,
    IReadOnlyList<OrderDto> RecentOrders);
