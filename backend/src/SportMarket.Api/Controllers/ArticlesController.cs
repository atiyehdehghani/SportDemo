using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportMarket.Application.Common.Interfaces;
using SportMarket.Application.Features.Articles;

namespace SportMarket.Api.Controllers;

public class ArticleForm
{
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
    public string? Slug { get; set; }
    public string? CoverImageUrl { get; set; }
    public IFormFile? CoverImageFile { get; set; }
}

[ApiController]
[Route("api/articles")]
public class ArticlesController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IFileStorageService _fileStorage;

    public ArticlesController(ISender sender, IFileStorageService fileStorage)
    {
        _sender = sender;
        _fileStorage = fileStorage;
    }

    [HttpGet]
    public async Task<IActionResult> GetArticles(
        [FromQuery] string? search,
        [FromQuery] bool includeUnpublished = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12)
    {
        var result = await _sender.Send(new GetArticlesQuery(search, includeUnpublished, page, pageSize));
        return Ok(result);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, [FromQuery] bool includeUnpublished = false)
    {
        var result = await _sender.Send(new GetArticleBySlugQuery(slug, includeUnpublished));
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("id/{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _sender.Send(new GetArticleByIdQuery(id));
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] ArticleForm form)
    {
        var command = new CreateArticleCommand(
            form.Title,
            form.Summary,
            form.Content,
            form.IsPublished,
            form.Slug,
            await _fileStorage.SaveAsync(form.CoverImageFile?.OpenReadStream(), form.CoverImageFile?.FileName, FileKind.Image));

        var result = await _sender.Send(command);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromForm] ArticleForm form)
    {
        var command = new UpdateArticleCommand(
            id,
            form.Title,
            form.Summary,
            form.Content,
            form.IsPublished,
            form.Slug,
            await _fileStorage.SaveAsync(form.CoverImageFile?.OpenReadStream(), form.CoverImageFile?.FileName, FileKind.Image) ?? form.CoverImageUrl);

        var result = await _sender.Send(command);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _sender.Send(new DeleteArticleCommand(id));
        return NoContent();
    }
}
