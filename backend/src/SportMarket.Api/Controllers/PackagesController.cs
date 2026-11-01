using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportMarket.Application.Common.Interfaces;
using SportMarket.Application.Features.Packages;

namespace SportMarket.Api.Controllers;

/// <summary>مدل فرم ایجاد/ویرایش پکیج (فایل‌ها اختیاری؛ در ویرایش مسیر قبلی حفظ می‌شود)</summary>
public class PackageForm
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? DiscountedPrice { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Slug { get; set; }
    public string? ImageUrl { get; set; }
    public string? FileUrl { get; set; }
    public IFormFile? ImageFile { get; set; }
    public IFormFile? PackageFile { get; set; }
}

[ApiController]
[Route("api/packages")]
public class PackagesController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IFileStorageService _fileStorage;

    public PackagesController(ISender sender, IFileStorageService fileStorage)
    {
        _sender = sender;
        _fileStorage = fileStorage;
    }

    /// <summary>لیست پکیج‌ها (برای ادمین includeInactive=true)</summary>
    [HttpGet]
    public async Task<IActionResult> GetPackages(
        [FromQuery] string? search,
        [FromQuery] bool includeInactive = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12)
    {
        var result = await _sender.Send(new GetPackagesQuery(search, includeInactive, page, pageSize));
        return Ok(result);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, [FromQuery] bool includeInactive = false)
    {
        var result = await _sender.Send(new GetPackageBySlugQuery(slug, includeInactive));
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("id/{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _sender.Send(new GetPackageByIdQuery(id));
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [RequestSizeLimit(26_214_400)]
    public async Task<IActionResult> Create([FromForm] PackageForm form)
    {
        var command = new CreatePackageCommand(
            form.Title,
            form.Description,
            form.Price,
            form.DiscountedPrice,
            form.IsActive,
            form.Slug,
            await _fileStorage.SaveAsync(form.ImageFile?.OpenReadStream(), form.ImageFile?.FileName, FileKind.Image),
            await _fileStorage.SaveAsync(form.PackageFile?.OpenReadStream(), form.PackageFile?.FileName, FileKind.PackageFile));

        var result = await _sender.Send(command);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    [RequestSizeLimit(26_214_400)]
    public async Task<IActionResult> Update(Guid id, [FromForm] PackageForm form)
    {
        var command = new UpdatePackageCommand(
            id,
            form.Title,
            form.Description,
            form.Price,
            form.DiscountedPrice,
            form.IsActive,
            form.Slug,
            await _fileStorage.SaveAsync(form.ImageFile?.OpenReadStream(), form.ImageFile?.FileName, FileKind.Image) ?? form.ImageUrl,
            await _fileStorage.SaveAsync(form.PackageFile?.OpenReadStream(), form.PackageFile?.FileName, FileKind.PackageFile) ?? form.FileUrl);

        var result = await _sender.Send(command);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _sender.Send(new DeletePackageCommand(id));
        return NoContent();
    }
}
