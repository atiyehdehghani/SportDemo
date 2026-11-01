using FluentValidation;
using MediatR;
using SportMarket.Application.Common.Dtos;
using SportMarket.Application.Common.Exceptions;
using SportMarket.Application.Common.Interfaces;

namespace SportMarket.Application.Features.Auth;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
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
    }
}

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    private readonly IIdentityService _identityService;
    private readonly IJwtService _jwtService;

    public RegisterCommandHandler(IIdentityService identityService, IJwtService jwtService)
    {
        _identityService = identityService;
        _jwtService = jwtService;
    }

    public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existing = await _identityService.FindByEmailAsync(request.Email, cancellationToken);
        if (existing is not null)
        {
            throw new ConflictException("این ایمیل قبلاً ثبت شده است.");
        }

        var result = await _identityService.CreateUserAsync(request.Email, request.Password, request.FullName, "User", cancellationToken);
        if (!result.Succeeded)
        {
            throw new AppValidationException(new Dictionary<string, string[]>
            {
                ["Password"] = result.Errors.ToArray()
            });
        }

        var user = (await _identityService.FindByEmailAsync(request.Email, cancellationToken))!;

        var (token, expiresAtUtc) = await _jwtService.GenerateTokenAsync(user.Id, user.Email, user.FullName, user.Roles);

        return new AuthResponseDto(token, expiresAtUtc, new UserDto(user.Id, user.Email, user.FullName, user.IsActive, user.Roles, user.CreatedAtUtc));
    }
}
