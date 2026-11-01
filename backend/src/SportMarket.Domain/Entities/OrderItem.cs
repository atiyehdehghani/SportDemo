using SportMarket.Domain.Common;

namespace SportMarket.Domain.Entities;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }

    public Order Order { get; set; } = null!;

    public Guid PackageId { get; set; }

    /// <summary>عنوان پکیج در لحظه خرید (تا با تغییر پکیج، سفارش عوض نشود)</summary>
    public string PackageTitle { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; } = 1;
}
