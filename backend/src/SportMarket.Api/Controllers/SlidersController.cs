using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportMarket.Application.Common.Interfaces;
using SportMarket.Application.Features.Sliders;

namespace SportMarket.Api.Controllers;

public class SliderForm
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public string? ImageUrl { get; set; }
    public IFormFile? ImageFile { get; set; }
}

[ApiController]
[Route("api/sliders")]
public class SlidersController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IFileStorageService _fileStorage;

    public SlidersController(ISender sender, IFileStorageService fileStorage)
    {
        _sender = sender;
        _fileStorage = fileStorage;
    }

    /// <summary>اسلایدهای فعال صفحه اصلی</summary>
    [HttpGet]
    public async Task<IActionResult> GetSliders()
    {
        var result = await _sender.Send(new GetSlidersQuery());
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _sender.Send(new GetSlidersQuery(IncludeAll: true));
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] SliderForm form)
    {
        var command = new CreateSliderCommand(
            form.Title,
            form.Subtitle,
            await _fileStorage.SaveAsync(form.ImageFile?.OpenReadStream(), form.ImageFile?.FileName, FileKind.Image)
                ?? form.ImageUrl
                ?? throw new Application.Common.Exceptions.ValidationException(
                    new Dictionary<string, string[]> { ["ImageFile"] = new[] { "تصویر اسلاید الزامی است." } }),
            form.LinkUrl,
            form.DisplayOrder,
            form.IsActive);

        var result = await _sender.Send(command);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromForm] SliderForm form)
    {
        var command = new UpdateSliderCommand(
            id,
            form.Title,
            form.Subtitle,
            await _fileStorage.SaveAsync(form.ImageFile?.OpenReadStream(), form.ImageFile?.FileName, FileKind.Image) ?? form.ImageUrl ?? string.Empty,
            form.LinkUrl,
            form.DisplayOrder,
            form.IsActive);

        var result = await _sender.Send(command);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _sender.Send(new DeleteSliderCommand(id));
        return NoContent();
    }
}
