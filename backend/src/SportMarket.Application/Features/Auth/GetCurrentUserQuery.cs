using MediatR;
using SportMarket.Application.Common.Dtos;
using SportMarket.Application.Common.Exceptions;
using SportMarket.Application.Common.Interfaces;

namespace SportMarket.Application.Features.Auth;

public record GetCurrentUserQuery : IRequest<UserDto>;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, UserDto>
{
    private readonly IIdentityService _identityService;
    private readonly ICurrentUserService _currentUser;

    public GetCurrentUserQueryHandler(IIdentityService identityService, ICurrentUserService currentUser)
    {
        _identityService = identityService;
        _currentUser = currentUser;
    }

    public async Task<UserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_currentUser.UserId))
        {
            throw new UnauthorizedException("برای مشاهده پروفایل باید وارد شوید.");
        }

        var user = await _identityService.GetSnapshotAsync(_currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException("کاربر", _currentUser.UserId);

        return new UserDto(user.Id, user.Email, user.FullName, user.IsActive, user.Roles, user.CreatedAtUtc);
    }
}
