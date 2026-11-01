using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SportMarket.Application.Common.Dtos;
using SportMarket.Application.Common.Exceptions;
using SportMarket.Application.Common.Interfaces;
using SportMarket.Domain.Entities;

namespace SportMarket.Application.Features.Sliders;

public record SliderMappings
{
    public static SliderDto ToDto(Slider s) =>
        new(s.Id, s.Title, s.Subtitle, s.ImageUrl, s.LinkUrl, s.DisplayOrder, s.IsActive);
}

public record GetSlidersQuery(bool IncludeAll = false) : IRequest<IReadOnlyList<SliderDto>>;

public class GetSlidersQueryHandler : IRequestHandler<GetSlidersQuery, IReadOnlyList<SliderDto>>
{
    private readonly IApplicationDbContext _db;

    public GetSlidersQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<SliderDto>> Handle(GetSlidersQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Sliders.AsNoTracking();

        if (!request.IncludeAll)
        {
            query = query.Where(s => s.IsActive);
        }

        var items = await query
            .OrderBy(s => s.DisplayOrder)
            .ToListAsync(cancellationToken);

        return items.Select(SliderMappings.ToDto).ToList();
    }
}

public record CreateSliderCommand(
    string Title,
    string Subtitle,
    string ImageUrl,
    string? LinkUrl,
    int DisplayOrder,
    bool IsActive) : IRequest<SliderDto>;

public class CreateSliderCommandValidator : AbstractValidator<CreateSliderCommand>
{
    public CreateSliderCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("عنوان اسلاید الزامی است.")
            .MaximumLength(200).WithMessage("عنوان حداکثر ۲۰۰ کاراکتر است.");

        RuleFor(x => x.Subtitle)
            .MaximumLength(300).WithMessage("زیرعنوان حداکثر ۳۰۰ کاراکتر است.");

        RuleFor(x => x.ImageUrl)
            .NotEmpty().WithMessage("تصویر اسلاید الزامی است.");
    }
}

public class CreateSliderCommandHandler : IRequestHandler<CreateSliderCommand, SliderDto>
{
    private readonly IApplicationDbContext _db;

    public CreateSliderCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<SliderDto> Handle(CreateSliderCommand request, CancellationToken cancellationToken)
    {
        var slider = new Slider
        {
            Title = request.Title,
            Subtitle = request.Subtitle,
            ImageUrl = request.ImageUrl,
            LinkUrl = request.LinkUrl,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive
        };

        _db.Sliders.Add(slider);
        await _db.SaveChangesAsync(cancellationToken);

        return SliderMappings.ToDto(slider);
    }
}

public record UpdateSliderCommand(
    Guid Id,
    string Title,
    string Subtitle,
    string ImageUrl,
    string? LinkUrl,
    int DisplayOrder,
    bool IsActive) : IRequest<SliderDto>;

public class UpdateSliderCommandValidator : AbstractValidator<UpdateSliderCommand>
{
    public UpdateSliderCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("عنوان اسلاید الزامی است.");

        RuleFor(x => x.ImageUrl)
            .NotEmpty().WithMessage("تصویر اسلاید الزامی است.");
    }
}

public class UpdateSliderCommandHandler : IRequestHandler<UpdateSliderCommand, SliderDto>
{
    private readonly IApplicationDbContext _db;

    public UpdateSliderCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<SliderDto> Handle(UpdateSliderCommand request, CancellationToken cancellationToken)
    {
        var slider = await _db.Sliders.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("اسلایدر", request.Id);

        slider.Title = request.Title;
        slider.Subtitle = request.Subtitle;
        slider.ImageUrl = request.ImageUrl;
        slider.LinkUrl = request.LinkUrl;
        slider.DisplayOrder = request.DisplayOrder;
        slider.IsActive = request.IsActive;

        await _db.SaveChangesAsync(cancellationToken);

        return SliderMappings.ToDto(slider);
    }
}

public record DeleteSliderCommand(Guid Id) : IRequest;

public class DeleteSliderCommandHandler : IRequestHandler<DeleteSliderCommand>
{
    private readonly IApplicationDbContext _db;

    public DeleteSliderCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task Handle(DeleteSliderCommand request, CancellationToken cancellationToken)
    {
        var slider = await _db.Sliders.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("اسلایدر", request.Id);

        _db.Sliders.Remove(slider);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
