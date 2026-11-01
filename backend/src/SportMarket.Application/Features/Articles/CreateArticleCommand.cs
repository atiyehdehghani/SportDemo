using FluentValidation;
using MediatR;
using SportMarket.Application.Common.Dtos;
using SportMarket.Application.Common.Exceptions;
using SportMarket.Application.Common.Helpers;
using SportMarket.Application.Common.Interfaces;
using SportMarket.Domain.Entities;

namespace SportMarket.Application.Features.Articles;

public record ArticleMappings
{
    public static ArticleDto ToDto(Article a) =>
        new(a.Id, a.Title, a.Slug, a.Summary, a.Content, a.CoverImageUrl, a.IsPublished, a.PublishedAtUtc, a.CreatedAtUtc);
}

public record CreateArticleCommand(
    string Title,
    string Summary,
    string Content,
    bool IsPublished,
    string? Slug,
    string? CoverImageUrl) : IRequest<ArticleDto>;

public class CreateArticleCommandValidator : AbstractValidator<CreateArticleCommand>
{
    public CreateArticleCommandValidator()
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

public class CreateArticleCommandHandler : IRequestHandler<CreateArticleCommand, ArticleDto>
{
    private readonly IApplicationDbContext _db;

    public CreateArticleCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ArticleDto> Handle(CreateArticleCommand request, CancellationToken cancellationToken)
    {
        var baseSlug = string.IsNullOrWhiteSpace(request.Slug)
            ? SlugHelper.Generate(request.Title, "article")
            : SlugHelper.Slugify(request.Slug);
        if (string.IsNullOrWhiteSpace(baseSlug))
        {
            baseSlug = SlugHelper.Generate(request.Title, "article");
        }

        var slug = baseSlug;
        var counter = 2;
        while (await _db.Articles.AnyAsync(a => a.Slug == slug, cancellationToken))
        {
            slug = $"{baseSlug}-{counter++}";
        }

        var article = new Article
        {
            Title = request.Title,
            Slug = slug,
            Summary = request.Summary,
            Content = request.Content,
            IsPublished = request.IsPublished,
            PublishedAtUtc = request.IsPublished ? DateTime.UtcNow : null,
            CoverImageUrl = request.CoverImageUrl
        };

        _db.Articles.Add(article);
        await _db.SaveChangesAsync(cancellationToken);

        return ArticleMappings.ToDto(article);
    }
}
