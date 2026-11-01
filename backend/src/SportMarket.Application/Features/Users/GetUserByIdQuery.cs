using MediatR;
using SportMarket.Application.Common.Dtos;
using SportMarket.Application.Common.Exceptions;
using SportMarket.Application.Common.Interfaces;

namespace SportMarket.Application.Features.Users;

public record GetUserByIdQuery(string Id) : IRequest<UserDto>;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto>
{
    private readonly IIdentityService _identityService;

    public GetUserByIdQueryHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _identityService.GetSnapshotAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("کاربر", request.Id);

        return new UserDto(user.Id, user.Email, user.FullName, user.IsActive, user.Roles, user.CreatedAtUtc);
    }
}
