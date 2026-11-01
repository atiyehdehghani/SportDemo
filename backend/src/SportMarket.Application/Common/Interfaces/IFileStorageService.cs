namespace SportMarket.Application.Common.Interfaces;

public enum FileKind
{
    Image,
    PackageFile
}

/// <summary>ذخیره فایل‌های آپلودی — پیاده‌سازی در Infrastructure</summary>
public interface IFileStorageService
{
    /// <summary>فایل را ذخیره و مسیر نسبی عمومی (مثل /uploads/...) را برمی‌گرداند؛ محتوای null نتیجه null می‌دهد</summary>
    Task<string?> SaveAsync(Stream? content, string? originalFileName, FileKind kind, CancellationToken ct = default);
}
