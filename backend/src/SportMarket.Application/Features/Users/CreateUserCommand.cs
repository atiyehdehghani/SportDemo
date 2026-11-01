using FluentValidation;
using MediatR;
using SportMarket.Application.Common.Dtos;
using SportMarket.Application.Common.Exceptions;
using SportMarket.Application.Common.Interfaces;

namespace SportMarket.Application.Features.Users;

public record CreateUserCommand(string FullName, string Email, string Password, string Role) : IRequest<UserDto>;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("نام و نام خانوادگی الزامی است.")
            .MinimumLength(3).WithMessage("نام و نام خانوادگی باید حداقل ۳ کاراکتر باشد.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("ایمیل الزامی است.")
            .EmailAddress().WithMessage("ایمیل معتبر نیست.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("رمز عبور الزامی است.")
            .MinimumLength(6).WithMessage("رمز عبور باید حداقل ۶ کاراکتر باشد.");

        RuleFor(x => x.Role)
            .Must(r => r is "Admin" or "User").WithMessage("نقش باید Admin یا User باشد.");
    }
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly IIdentityService _identityService;

    public CreateUserCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var existing = await _identityService.FindByEmailAsync(request.Email, cancellationToken);
        if (existing is not null)
        {
            throw new ConflictException("این ایمیل قبلاً ثبت شده است.");
        }

        var result = await _identityService.CreateUserAsync(request.Email, request.Password, request.FullName, request.Role, cancellationToken);
        if (!result.Succeeded)
        {
            throw new AppValidationException(new Dictionary<string, string[]> { ["Password"] = result.Errors.ToArray() });
        }

        var user = (await _identityService.FindByEmailAsync(request.Email, cancellationToken))!;

        return new UserDto(user.Id, user.Email, user.FullName, user.IsActive, user.Roles, user.CreatedAtUtc);
    }
}
