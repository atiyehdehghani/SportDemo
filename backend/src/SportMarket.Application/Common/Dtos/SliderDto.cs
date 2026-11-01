namespace SportMarket.Application.Common.Dtos;

public record SliderDto(
    Guid Id,
    string Title,
    string Subtitle,
    string ImageUrl,
    string? LinkUrl,
    int DisplayOrder,
    bool IsActive);
