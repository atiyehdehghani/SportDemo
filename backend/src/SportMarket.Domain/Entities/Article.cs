using SportMarket.Domain.Common;

namespace SportMarket.Domain.Entities;

public class Article : BaseEntity
{
    public string Title { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string? CoverImageUrl { get; set; }

    public bool IsPublished { get; set; }

    public DateTime? PublishedAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
