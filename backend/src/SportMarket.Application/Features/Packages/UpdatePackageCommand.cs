using FluentValidation;
using MediatR;
using SportMarket.Application.Common.Dtos;
using SportMarket.Application.Common.Exceptions;
using SportMarket.Application.Common.Interfaces;
using SportMarket.Application.Features.Packages;

namespace SportMarket.Application.Features.Packages;

public record UpdatePackageCommand(
    Guid Id,
    string Title,
    string Description,
    decimal Price,
    decimal? DiscountedPrice,
    bool IsActive,
    string? Slug,
    string? ImageUrl,
    string? FileUrl) : IRequest<PackageDto>;

public class UpdatePackageCommandValidator : AbstractValidator<UpdatePackageCommand>
{
    public UpdatePackageCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("عنوان پکیج الزامی است.")
            .MaximumLength(200).WithMessage("عنوان حداکثر ۲۰۰ کاراکتر است.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("توضیحات الزامی است.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("قیمت نمی‌تواند منفی باشد.");

        RuleFor(x => x.DiscountedPrice)
            .GreaterThanOrEqualTo(0).WithMessage("قیمت با تخفیف نمی‌تواند منفی باشد.")
            .LessThanOrEqualTo(x => x.Price).WithMessage("قیمت با تخفیف باید کمتر یا مساوی قیمت اصلی باشد.")
            .When(x => x.DiscountedPrice.HasValue);
    }
}

public class UpdatePackageCommandHandler : IRequestHandler<UpdatePackageCommand, PackageDto>
{
    private readonly IApplicationDbContext _db;

    public UpdatePackageCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PackageDto> Handle(UpdatePackageCommand request, CancellationToken cancellationToken)
    {
        var package = await _db.Packages.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("پکیج", request.Id);

        package.Title = request.Title;
        package.Description = request.Description;
        package.Price = request.Price;
        package.DiscountedPrice = request.DiscountedPrice;
        package.IsActive = request.IsActive;
        package.ImageUrl = request.ImageUrl;
        package.FileUrl = request.FileUrl;

        var requestedSlug = string.IsNullOrWhiteSpace(request.Slug) ? null : SlugHelper.Slugify(request.Slug);
        if (requestedSlug != package.Slug)
        {
            package.Slug = await PackageMappings.EnsureUniqueSlugAsync(_db, requestedSlug, request.Title, cancellationToken);
        }

        await _db.SaveChangesAsync(cancellationToken);

        return PackageMappings.ToDto(package);
    }
}
