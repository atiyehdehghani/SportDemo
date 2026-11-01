using SportMarket.Domain.Common;

namespace SportMarket.Domain.Entities;

public class Slider : BaseEntity
{
    public string Title { get; set; } = string.Empty;

    public string Subtitle { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    public string? LinkUrl { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;
}
