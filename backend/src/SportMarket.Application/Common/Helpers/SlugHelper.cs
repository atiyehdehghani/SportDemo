using System.Text.RegularExpressions;

namespace SportMarket.Application.Common.Helpers;

public static partial class SlugHelper
{
    [GeneratedRegex(@"[^a-z0-9\-]+")]
    private static partial Regex InvalidCharsRegex();

    /// <summary>اسلاگ انگلیسی می‌سازد؛ اگر عنوان فارسی باشد خروجی تهی است</summary>
    public static string Slugify(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        var value = input.Trim().ToLowerInvariant().Replace(' ', '-');
        return InvalidCharsRegex().Replace(value, string.Empty).Trim('-');
    }

    /// <summary>اسلاگ معتبر می‌سازد؛ برای عناوین فارسی از پیشوند + شناسه تصادفی استفاده می‌کند</summary>
    public static string Generate(string title, string fallbackPrefix = "item")
    {
        var slug = Slugify(title);
        if (!string.IsNullOrWhiteSpace(slug))
        {
            return slug;
        }

        return $"{fallbackPrefix}-{Guid.NewGuid().ToString("N")[..10]}";
    }
}
