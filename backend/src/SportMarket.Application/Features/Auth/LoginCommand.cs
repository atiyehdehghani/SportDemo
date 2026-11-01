using FluentValidation;
using MediatR;
using SportMarket.Application.Common.Dtos;
using SportMarket.Application.Common.Exceptions;
using SportMarket.Application.Common.Interfaces;

namespace SportMarket.Application.Features.Auth;

public record LoginCommand(string Email, string Password) : IRequest<AuthResponseDto>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("ایمیل الزامی است.")
            .EmailAddress().WithMessage("ایمیل معتبر نیست.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("رمز عبور الزامی است.");
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IIdentityService _identityService;
    private readonly IJwtService _jwtService;

    public LoginCommandHandler(IIdentityService identityService, IJwtService jwtService)
    {
        _identityService = identityService;
        _jwtService = jwtService;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var authResult = await _identityService.AuthenticateAsync(request.Email, request.Password, cancellationToken);
        if (authResult is null)
        {
            throw new UnauthorizedException("ایمیل یا رمز عبور نادرست است.");
        }

        var (user, roles) = authResult.Value;
        if (!user.IsActive)
        {
            throw new UnauthorizedException("حساب کاربری شما غیرفعال شده است.");
        }

        var (token, expiresAtUtc) = await _jwtService.GenerateTokenAsync(user.Id, user.Email, user.FullName, roles);

        return new AuthResponseDto(token, expiresAtUtc, new UserDto(user.Id, user.Email, user.FullName, user.IsActive, roles, user.CreatedAtUtc));
    }
}
