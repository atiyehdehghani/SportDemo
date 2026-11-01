namespace SportMarket.Application.Common.Dtos;

public record PackageDto(
    Guid Id,
    string Title,
    string Slug,
    string Description,
    decimal Price,
    decimal? DiscountedPrice,
    string? ImageUrl,
    string? FileUrl,
    bool IsActive,
    DateTime CreatedAtUtc)
{
    /// <summary>قیمت نهایی پرداختی (با احتساب تخفیف)</summary>
    public decimal EffectivePrice => DiscountedPrice ?? Price;
}
