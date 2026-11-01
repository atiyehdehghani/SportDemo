using MediatR;
using SportMarket.Application.Common.Exceptions;
using SportMarket.Application.Common.Interfaces;

namespace SportMarket.Application.Features.Users;

public record DeleteUserCommand(string Id) : IRequest;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
{
    private readonly IIdentityService _identityService;
    private readonly ICurrentUserService _currentUser;

    public DeleteUserCommandHandler(IIdentityService identityService, ICurrentUserService currentUser)
    {
        _identityService = identityService;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        if (request.Id == _currentUser.UserId)
        {
            throw new ConflictException("حذف حساب کاربری خودتان امکان‌پذیر نیست.");
        }

        var user = await _identityService.GetSnapshotAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("کاربر", request.Id);

        var result = await _identityService.DeleteUserAsync(request.Id, cancellationToken);
        if (!result.Succeeded)
        {
            throw new ConflictException(string.Join(" ", result.Errors));
        }
    }
}
