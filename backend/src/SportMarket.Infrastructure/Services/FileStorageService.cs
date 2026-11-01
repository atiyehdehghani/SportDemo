using Microsoft.AspNetCore.Hosting;
using SportMarket.Application.Common.Exceptions;
using SportMarket.Application.Common.Interfaces;

namespace SportMarket.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".svg" };
    private static readonly string[] PackageFileExtensions = { ".pdf", ".zip", ".rar", ".mp4", ".mp3", ".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx" };

    private readonly IWebHostEnvironment _environment;

    public FileStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string?> SaveAsync(Stream? content, string? originalFileName, FileKind kind, CancellationToken ct = default)
    {
        if (content is null || content.Length == 0 || string.IsNullOrWhiteSpace(originalFileName))
        {
            return null;
        }

        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        var allowedExtensions = kind == FileKind.Image ? ImageExtensions : PackageFileExtensions;

        if (!allowedExtensions.Contains(extension))
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["File"] = new[] { kind == FileKind.Image
                    ? "فرمت تصویر مجاز نیست. (jpg، png، webp، gif، svg)"
                    : "فرمت فایل مجاز نیست. (pdf، zip، rar، mp4، doc و ...)" }
            });
        }

        var webRoot = _environment.WebRootPath;
        if (string.IsNullOrEmpty(webRoot))
        {
            webRoot = Path.Combine(_environment.ContentRootPath, "wwwroot");
        }

        var now = DateTime.UtcNow;
        var relativeFolder = Path.Combine("uploads",
            kind == FileKind.Image ? "images" : "files",
            now.ToString("yyyy"),
            now.ToString("MM"));

        var absoluteFolder = Path.Combine(webRoot, relativeFolder);
        Directory.CreateDirectory(absoluteFolder);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var absolutePath = Path.Combine(absoluteFolder, fileName);

        await using var output = new FileStream(absolutePath, FileMode.Create);
        await content.CopyToAsync(output, ct);

        return "/" + Path.Combine(relativeFolder, fileName).Replace('\\', '/');
    }
}
