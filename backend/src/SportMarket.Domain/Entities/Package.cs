using SportMarket.Domain.Common;

namespace SportMarket.Domain.Entities;

public class Package : BaseEntity
{
    public string Title { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public decimal? DiscountedPrice { get; set; }

    public string? ImageUrl { get; set; }

    /// <summary>فایل قابل دانلود پکیج (مثل pdf یا zip)</summary>
    public string? FileUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
