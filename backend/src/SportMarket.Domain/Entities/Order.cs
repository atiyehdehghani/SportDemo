using SportMarket.Domain.Common;
using SportMarket.Domain.Enums;

namespace SportMarket.Domain.Entities;

public class Order : BaseEntity
{
    public string UserId { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? PaidAtUtc { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
