using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SportMarket.Application.Common.Dtos;
using SportMarket.Application.Common.Exceptions;
using SportMarket.Application.Common.Helpers;
using SportMarket.Application.Features.Articles;

namespace SportMarket.Application.Features.Articles;

public record UpdateArticleCommand(
    Guid Id,
    string Title,
    string Summary,
    string Content,
    bool IsPublished,
    string? Slug,
    string? CoverImageUrl) : IRequest<ArticleDto>;

public class UpdateArticleCommandValidator : AbstractValidator<UpdateArticleCommand>
{
    public UpdateArticleCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("عنوان مقاله الزامی است.")
            .MaximumLength(200).WithMessage("عنوان حداکثر ۲۰۰ کاراکتر است.");

        RuleFor(x => x.Summary)
            .NotEmpty().WithMessage("خلاصه الزامی است.")
            .MaximumLength(500).WithMessage("خلاصه حداکثر ۵۰۰ کاراکتر است.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("متن مقاله الزامی است.");
    }
}

public class UpdateArticleCommandHandler : IRequestHandler<UpdateArticleCommand, ArticleDto>
{
    private readonly IApplicationDbContext _db;

    public UpdateArticleCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ArticleDto> Handle(UpdateArticleCommand request, CancellationToken cancellationToken)
    {
        var article = await _db.Articles.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("مقاله", request.Id);

        article.Title = request.Title;
        article.Summary = request.Summary;
        article.Content = request.Content;
        article.CoverImageUrl = request.CoverImageUrl;

        if (request.IsPublished && !article.IsPublished)
        {
            article.PublishedAtUtc = DateTime.UtcNow;
        }
        article.IsPublished = request.IsPublished;

        var requestedSlug = string.IsNullOrWhiteSpace(request.Slug) ? null : SlugHelper.Slugify(request.Slug);
        if (requestedSlug != article.Slug)
        {
            var baseSlug = requestedSlug ?? SlugHelper.Generate(request.Title, "article");
            var slug = baseSlug;
            var counter = 2;
            while (await _db.Articles.AnyAsync(a => a.Slug == slug && a.Id != article.Id, cancellationToken))
            {
                slug = $"{baseSlug}-{counter++}";
            }
            article.Slug = slug;
        }

        await _db.SaveChangesAsync(cancellationToken);

        return ArticleMappings.ToDto(article);
    }
}

public record DeleteArticleCommand(Guid Id) : IRequest;

public class DeleteArticleCommandHandler : IRequestHandler<DeleteArticleCommand>
{
    private readonly IApplicationDbContext _db;

    public DeleteArticleCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task Handle(DeleteArticleCommand request, CancellationToken cancellationToken)
    {
        var article = await _db.Articles.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("مقاله", request.Id);

        _db.Articles.Remove(article);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
