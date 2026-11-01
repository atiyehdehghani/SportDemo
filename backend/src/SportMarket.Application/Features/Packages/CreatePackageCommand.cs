using FluentValidation;
using MediatR;
using SportMarket.Application.Common.Dtos;
using SportMarket.Application.Common.Exceptions;
using SportMarket.Application.Common.Helpers;
using SportMarket.Application.Common.Interfaces;
using SportMarket.Domain.Entities;

namespace SportMarket.Application.Features.Packages;

public record CreatePackageCommand(
    string Title,
    string Description,
    decimal Price,
    decimal? DiscountedPrice,
    bool IsActive,
    string? Slug,
    string? ImageUrl,
    string? FileUrl) : IRequest<PackageDto>;

public class CreatePackageCommandValidator : AbstractValidator<CreatePackageCommand>
{
    public CreatePackageCommandValidator()
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

public class CreatePackageCommandHandler : IRequestHandler<CreatePackageCommand, PackageDto>
{
    private readonly IApplicationDbContext _db;

    public CreatePackageCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PackageDto> Handle(CreatePackageCommand request, CancellationToken cancellationToken)
    {
        var package = new Package
        {
            Title = request.Title,
            Slug = await PackageMappings.EnsureUniqueSlugAsync(_db, request.Slug, request.Title, cancellationToken),
            Description = request.Description,
            Price = request.Price,
            DiscountedPrice = request.DiscountedPrice,
            ImageUrl = request.ImageUrl,
            FileUrl = request.FileUrl,
            IsActive = request.IsActive
        };

        _db.Packages.Add(package);
        await _db.SaveChangesAsync(cancellationToken);

        return PackageMappings.ToDto(package);
    }
}
