using MediatR;
using SportMarket.Application.Common.Dtos;
using SportMarket.Application.Common.Interfaces;
using SportMarket.Application.Common.Models;

namespace SportMarket.Application.Features.Users;

public record GetUsersQuery(string? Search, int Page = 1, int PageSize = 10) : IRequest<PagedResult<UserDto>>;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PagedResult<UserDto>>
{
    private readonly IIdentityService _identityService;

    public GetUsersQueryHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<PagedResult<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var page = await _identityService.GetUsersAsync(request.Search, request.Page, request.PageSize, cancellationToken);

        return new PagedResult<UserDto>(
            page.Items.Select(u => new UserDto(u.Id, u.Email, u.FullName, u.IsActive, u.Roles, u.CreatedAtUtc)).ToList(),
            page.TotalCount, page.Page, page.PageSize);
    }
}
