namespace SportMarket.Application.Common.Dtos;

public record ArticleDto(
    Guid Id,
    string Title,
    string Slug,
    string Summary,
    string Content,
    string? CoverImageUrl,
    bool IsPublished,
    DateTime? PublishedAtUtc,
    DateTime CreatedAtUtc);
