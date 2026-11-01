using FluentValidation;
using MediatR;
using SportMarket.Application.Common.Dtos;
using SportMarket.Application.Common.Exceptions;
using SportMarket.Application.Common.Interfaces;

namespace SportMarket.Application.Features.Users;

public record UpdateUserCommand(string Id, string FullName, bool IsActive, string Role, string? NewPassword) : IRequest;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("نام و نام خانوادگی الزامی است.")
            .MinimumLength(3).WithMessage("نام و نام خانوادگی باید حداقل ۳ کاراکتر باشد.");

        RuleFor(x => x.Role)
            .Must(r => r is "Admin" or "User").WithMessage("نقش باید Admin یا User باشد.");

        RuleFor(x => x.NewPassword)
            .MinimumLength(6).WithMessage("رمز عبور باید حداقل ۶ کاراکتر باشد.")
            .When(x => !string.IsNullOrEmpty(x.NewPassword));
    }
}

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand>
{
    private readonly IIdentityService _identityService;

    public UpdateUserCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _identityService.GetSnapshotAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("کاربر", request.Id);

        var profileResult = await _identityService.UpdateProfileAsync(request.Id, request.FullName, request.IsActive, cancellationToken);
        if (!profileResult.Succeeded)
        {
            throw new ConflictException(string.Join(" ", profileResult.Errors));
        }

        if (user.Roles.FirstOrDefault() != request.Role)
        {
            var roleResult = await _identityService.UpdateRoleAsync(request.Id, request.Role, cancellationToken);
            if (!roleResult.Succeeded)
            {
                throw new ConflictException(string.Join(" ", roleResult.Errors));
            }
        }

        if (!string.IsNullOrEmpty(request.NewPassword))
        {
            var passwordResult = await _identityService.SetPasswordAsync(request.Id, request.NewPassword, cancellationToken);
            if (!passwordResult.Succeeded)
            {
                throw new AppValidationException(new Dictionary<string, string[]> { ["NewPassword"] = passwordResult.Errors.ToArray() });
            }
        }
    }
}
