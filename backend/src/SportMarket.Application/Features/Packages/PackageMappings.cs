using SportMarket.Application.Common.Dtos;
using SportMarket.Application.Common.Helpers;
using SportMarket.Domain.Entities;

namespace SportMarket.Application.Features.Packages;

public static class PackageMappings
{
    public static PackageDto ToDto(Package p) =>
        new(p.Id, p.Title, p.Slug, p.Description, p.Price, p.DiscountedPrice, p.ImageUrl, p.FileUrl, p.IsActive, p.CreatedAtUtc);

    /// <summary>اسلاگ یکتا می‌سازد؛ اگر درخواستی تهی باشد از عنوان ساخته می‌شود</summary>
    public static async Task<string> EnsureUniqueSlugAsync(
        IApplicationDbContext db, string? requestedSlug, string title, CancellationToken ct)
    {
        var baseSlug = SlugHelper.Slugify(requestedSlug);
        if (string.IsNullOrWhiteSpace(baseSlug))
        {
            baseSlug = SlugHelper.Generate(title, "package");
        }

        var slug = baseSlug;
        var counter = 2;
        while (await db.Packages.AnyAsync(p => p.Slug == slug, ct))
        {
            slug = $"{baseSlug}-{counter++}";
        }

        return slug;
    }
}
